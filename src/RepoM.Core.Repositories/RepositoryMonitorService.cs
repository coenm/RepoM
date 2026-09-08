namespace RepoM.Core.Repositories;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DynamicData;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Persistence;
using RepoM.Core.Repositories.Reading;
using RepoM.Core.Repositories.Scanning;
using RepoM.Core.Repositories.Store;
using RepoM.Core.Repositories.Watching;

public sealed class RepositoryMonitorService : IModule, IDisposable
{
    private readonly IRepositoryScanner _scanner;
    private readonly IRepositoryWatcher _watcher;
    private readonly IRepositoryInfoReader _reader;
    private readonly IRepositoryStore _store;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly Func<IEnumerable<string>> _pathProvider;
    private readonly IRepositoryMonitoringService _monitoringState;
    private readonly IRepositoryMonitoringEvents _monitoringEvents;
    private readonly IRepositorySnapshotStore _snapshotStore;
    private readonly TimeSpan _scanInterval;
    private readonly TimeSpan _snapshotSaveDebounce;
    private readonly Lock _scanLock = new();
    private CompositeDisposable? _subscriptions;
    private CancellationTokenSource _scanCts = new();
    private bool _disposed;

    private readonly Lock _repoWatcherLock = new();
    private readonly Dictionary<string, IDisposable> _repoWatcherSubscriptions = new(StringComparer.OrdinalIgnoreCase);

    public RepositoryMonitorService(
        IRepositoryScanner scanner,
        IRepositoryWatcher watcher,
        IRepositoryInfoReader reader,
        IRepositoryStore store,
        IFileSystem fileSystem,
        Func<IEnumerable<string>> pathProvider,
        IRepositoryMonitoringService monitoringState,
        IRepositoryMonitoringEvents monitoringEvents,
        IRepositorySnapshotStore snapshotStore,
        ILogger logger)
    {
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _monitoringState = monitoringState ?? throw new ArgumentNullException(nameof(monitoringState));
        _monitoringEvents = monitoringEvents ?? throw new ArgumentNullException(nameof(monitoringEvents));
        _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scanInterval = TimeSpan.FromMinutes(30);
        _snapshotSaveDebounce = TimeSpan.FromSeconds(5);
    }

    public IRepositoryStore Store => _store;

    public IObservable<bool> IsScanning => _scanner.IsScanning;

    public async Task StartAsync()
    {
        var ctNone = CancellationToken.None; // this method does not support cancellation
        _logger.LogInformation("RepositoryMonitorService starting");

        _subscriptions = new CompositeDisposable();

        IReadOnlyList<RepositoryInfo> snapshot = await _snapshotStore.LoadAsync(ctNone).ConfigureAwait(false);
        _store.AddOrUpdateRange(snapshot);

        var snapshotSaveSubscription = _store
            .Connect()
            .Throttle(_snapshotSaveDebounce)
            .SelectMany(_ => Observable.FromAsync(token => _snapshotStore.SaveAsync(_store.Items, token)))
            .Subscribe(
                _ => { },
                ex => _logger.LogError(ex, "Could not save repository snapshot"));
        _subscriptions.Add(snapshotSaveSubscription);

        // Root-level watching is used for detecting newly created / changed repositories.
        // It also keeps our behavior consistent with the previous contract where the
        // watcher receives the configured search roots.
        var discoveryWatchSubscription = _watcher
            .Watch(_pathProvider())
            .Subscribe(
                OnRepositoryChangeDetected,
                ex => _logger.LogError(ex, "Error in repository watcher"));

        _subscriptions.Add(discoveryWatchSubscription);

        // Start filesystem watching for real-time detection.
        // Only set up watchers for repositories that are actively monitored.
        var repoWatcherSetupSubscription = _store
            .Connect()
            .Subscribe(changeSet =>
            {
                foreach (var change in changeSet)
                {
                    if (change.Reason == ChangeReason.Remove)
                    {
                        if (change.Previous.HasValue)
                        {
                            RemoveRepoWatcher(change.Previous.Value);
                        }
                        continue;
                    }

                    if (_monitoringState.IsMonitored(change.Current.SafePath))
                    {
                        EnsureRepoWatcher(change.Current);
                    }
                }
            }, ex => _logger.LogError(ex, "Error in repository watcher setup"));

        _subscriptions.Add(repoWatcherSetupSubscription);

        // React to monitoring state changes: add/remove watchers dynamically.
        _monitoringEvents.MonitoringChanged += OnMonitoringStateChanged;

        // Initial scan
        var initialScanSubscription = CreateScanPipeline(_scanCts.Token)
            .Subscribe(
                _ => { },
                ex => _logger.LogError(ex, "Error during initial scan"),
                () => _logger.LogInformation("Initial scan completed"));
        _subscriptions.Add(initialScanSubscription);

        // Periodic scan — only refreshes actively monitored repositories.
        if (_scanInterval > TimeSpan.Zero)
        {
            var periodicSubscription = Observable
                .Interval(_scanInterval)
                .SelectMany(_ =>
                {
                    CancellationToken token;
                    lock (_scanLock)
                    {
                        token = _scanCts.Token;
                    }

                    return CreateMonitoredRefreshPipeline(token);
                })
                .Subscribe(
                    _ => { },
                    ex => _logger.LogError(ex, "Error during periodic scan"));
            _subscriptions.Add(periodicSubscription);
        }

        // Periodic staleness check (every 60 seconds)
        var stalenessSubscription = Observable
            .Interval(TimeSpan.FromSeconds(60))
            .Subscribe(_ => RemoveStaleRepositories());
        _subscriptions.Add(stalenessSubscription);

        _logger.LogInformation("RepositoryMonitorService started");
    }

    /// <summary>
    /// Activates monitoring for a repository. Sets it as monitored and ensures
    /// a file system watcher is started.
    /// </summary>
    public void ActivateMonitoring(string safePath)
    {
        _monitoringState.SetMonitored(safePath, true);
    }

    /// <summary>
    /// Deactivates monitoring for a repository. Removes the file system watcher.
    /// </summary>
    public void DeactivateMonitoring(string safePath)
    {
        _monitoringState.SetMonitored(safePath, false);
    }

    private void OnMonitoringStateChanged(string safePath, bool monitored)
    {
        var existing = _store.Lookup(safePath);
        if (!existing.HasValue)
        {
            return;
        }

        if (monitored)
        {
            EnsureRepoWatcher(existing.Value);
        }
        else
        {
            RemoveRepoWatcher(existing.Value);
        }
    }

    private void EnsureRepoWatcher(RepositoryInfo? repo)
    {
        if (repo is null)
        {
            return;
        }

        var safePath = repo.SafePath;
        lock (_repoWatcherLock)
        {
            if (_repoWatcherSubscriptions.ContainsKey(safePath))
            {
                return;
            }
        }

        if (!TryResolveGitDir(repo.Path, out var gitDirPath))
        {
            return;
        }

        var repoPath = repo.Path;
        var subscription = _watcher
            .Watch([gitDirPath])
            .Subscribe(
                changeEvent => OnRepositoryChangeDetected(
                    new RepositoryChangeEvent(repoPath, changeEvent.ChangeType)),
                ex => _logger.LogError(ex, "Error in repository watcher"));

        lock (_repoWatcherLock)
        {
            // Re-check in case we raced.
            if (_repoWatcherSubscriptions.ContainsKey(safePath))
            {
                subscription.Dispose();
                return;
            }

            _repoWatcherSubscriptions.Add(safePath, subscription);
        }

        _subscriptions?.Add(subscription);
    }

    private void RemoveRepoWatcher(RepositoryInfo? repo)
    {
        if (repo is null)
        {
            return;
        }

        lock (_repoWatcherLock)
        {
            if (_repoWatcherSubscriptions.TryGetValue(repo.SafePath, out IDisposable? subscription))
            {
                subscription.Dispose();
                _repoWatcherSubscriptions.Remove(repo.SafePath);
            }
        }
    }

    private bool TryResolveGitDir(string repoRootPath, out string gitDirPath)
    {
        gitDirPath = string.Empty;

        // Normal clone:
        //   <repo>/.git/HEAD
        var candidateDir = _fileSystem.Path.Combine(repoRootPath, ".git");
        if (_fileSystem.Directory.Exists(candidateDir))
        {
            gitDirPath = candidateDir;
            return true;
        }

        // Worktree-style:
        //   <worktree>/.git is a file containing: gitdir: <path>
        //   (the <path> can be relative to the worktree root)
        if (!_fileSystem.File.Exists(candidateDir))
        {
            return false;
        }

        try
        {
            var lines = _fileSystem.File.ReadAllLines(candidateDir);
            var gitDirLine = lines.FirstOrDefault(l =>
                l.TrimStart().StartsWith("gitdir:", StringComparison.OrdinalIgnoreCase));

            if (gitDirLine is null)
            {
                return false;
            }

            var value = gitDirLine.Split(':', 2)[1].Trim();
            value = value.Trim('\"', '\'');

            if (!System.IO.Path.IsPathRooted(value))
            {
                value = System.IO.Path.GetFullPath(System.IO.Path.Combine(repoRootPath, value));
            }

            if (!_fileSystem.Directory.Exists(value))
            {
                return false;
            }

            gitDirPath = value;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task StopAsync()
    {
        _logger.LogInformation("RepositoryMonitorService stopping");

        _monitoringEvents.MonitoringChanged -= OnMonitoringStateChanged;
        _subscriptions?.Dispose();
        _subscriptions = null;

        // Persist the current set of repositories so the next startup can show them instantly.
        SaveSnapshotAsync().GetAwaiter().GetResult();

        _logger.LogInformation("RepositoryMonitorService stopped");
        return Task.CompletedTask;
    }

    public void CancelAllScans()
    {
        _logger.LogInformation("Cancelling all active scans");
        lock (_scanLock)
        {
            _scanCts.Cancel();
            _scanCts.Dispose();
            _scanCts = new CancellationTokenSource();
        }
    }

    public Task ScanAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Manual scan triggered");

        CancellationTokenSource linkedCts;
        lock (_scanLock)
        {
            linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, _scanCts.Token);
        }

        var tcs = new TaskCompletionSource();

        var subscription = CreateScanPipeline(linkedCts.Token)
            .Subscribe(
                _ => { },
                ex =>
                {
                    _logger.LogError(ex, "Error during manual scan");
                    tcs.TrySetException(ex);
                    linkedCts.Dispose();
                },
                () =>
                {
                    _logger.LogInformation("Manual scan completed");
                    tcs.TrySetResult();
                    linkedCts.Dispose();
                });

        linkedCts.Token.Register(() =>
        {
            subscription.Dispose();
            tcs.TrySetCanceled(CancellationToken.None);
        });

        return tcs.Task;
    }

    private IObservable<RepositoryInfo> CreateScanPipeline(CancellationToken ct = default)
    {
        return _scanner
            .Scan(_pathProvider(), ct)
            .SelectMany(path => Observable.FromAsync(async token =>
            {
                // Publish a lightweight entry immediately so newly discovered repositories show up
                // in the list right away, before the expensive git status read completes.
                var safePath = NormalizeToSafePath(path);
                var addedStub = TryPublishDiscoveredStub(path, safePath);

                RepositoryInfo? repo = await _reader.ReadAsync(path, token).ConfigureAwait(false);
                if (repo == null)
                {
                    // The read failed; drop the stub we optimistically added to avoid a phantom entry.
                    if (addedStub)
                    {
                        _store.Remove(safePath);
                    }

                    return null;
                }

                repo.LastSeen = DateTimeOffset.UtcNow;
                return repo;
            }))
            .Where(repo => repo != null)
            .Select(repo => repo!)
            .Buffer(TimeSpan.FromMilliseconds(250), 25)
            .Where(batch => batch.Count > 0)
            .Do(batch => _store.AddOrUpdateRange(batch))
            .SelectMany(batch => batch);
    }

    /// <summary>
    /// Adds a minimal, status-less repository entry to the store so it becomes visible immediately.
    /// Returns <c>true</c> when a new stub was added; <c>false</c> when the repository was already known
    /// (e.g. loaded from the snapshot or a previous scan) and must not be downgraded.
    /// </summary>
    private bool TryPublishDiscoveredStub(string discoveredPath, string safePath)
    {
        if (_store.Lookup(safePath).HasValue)
        {
            return false;
        }

        var repoRoot = GetRepositoryRoot(discoveredPath);
        var now = DateTimeOffset.UtcNow;
        var stub = new RepositoryInfo
        {
            Path = repoRoot,
            SafePath = safePath,
            Name = _fileSystem.Path.GetFileName(repoRoot),
            LastSeen = now,
            LastUpdated = DateTimeOffset.MinValue,
        };

        _store.AddOrUpdate(stub);
        return true;
    }

    private static string GetRepositoryRoot(string path)
    {
        var gitIndex = path.IndexOf(".git", StringComparison.OrdinalIgnoreCase);
        return gitIndex > 0
            ? path[..gitIndex].TrimEnd('\\', '/')
            : path;
    }

    private async Task SaveSnapshotAsync()
    {
        try
        {
            await _snapshotStore.SaveAsync(_store.Items.ToList()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to save repository snapshot");
        }
    }

    /// <summary>
    /// Creates a pipeline that only re-reads actively monitored repositories
    /// from the store rather than scanning the entire file system.
    /// </summary>
    private IObservable<RepositoryInfo> CreateMonitoredRefreshPipeline(CancellationToken ct = default)
    {
        var monitored = _store.Items
            .Where(r => _monitoringState.IsMonitored(r.SafePath))
            .ToList();

        if (monitored.Count == 0)
        {
            return Observable.Empty<RepositoryInfo>();
        }

        return monitored
            .ToObservable()
            .SelectMany(repo => Observable.FromAsync(async token =>
            {
                var headPath = _fileSystem.Path.Combine(repo.Path, ".git", "HEAD");
                RepositoryInfo? updated = await _reader.ReadAsync(headPath, token).ConfigureAwait(false);
                updated ??= await _reader.ReadAsync(repo.Path, token).ConfigureAwait(false);
                if (updated != null)
                {
                    updated.LastSeen = DateTimeOffset.UtcNow;
                    updated.LastUpdated = DateTimeOffset.UtcNow;
                }
                return updated;
            }))
            .Where(repo => repo != null)
            .Select(repo => repo!)
            .Buffer(TimeSpan.FromMilliseconds(500))
            .Where(batch => batch.Count > 0)
            .Do(batch => _store.AddOrUpdateRange(batch))
            .SelectMany(batch => batch);
    }

    private void OnRepositoryChangeDetected(RepositoryChangeEvent changeEvent)
    {
        switch (changeEvent.ChangeType)
        {
            case RepositoryChangeType.Added:
            case RepositoryChangeType.Modified:
                _logger.LogDebug("Repository change detected: {ChangeType} at {Path}", changeEvent.ChangeType, changeEvent.Path);
                _ = ReadAndUpdateRepositoryAsync(changeEvent.Path);
                break;

            case RepositoryChangeType.Removed:
                _logger.LogDebug("Repository removal detected at {Path}", changeEvent.Path);
                var safePath = NormalizeToSafePath(changeEvent.Path);
                _store.Remove(safePath);
                break;
        }
    }

    private async Task ReadAndUpdateRepositoryAsync(string path)
    {
        const int MAX_RETRIES = 3;

        for (var attempt = 0; attempt < MAX_RETRIES; attempt++)
        {
            try
            {
                RepositoryInfo? repo = await _reader.ReadAsync(path, CancellationToken.None).ConfigureAwait(false);
                if (repo != null)
                {
                    repo.LastSeen = DateTimeOffset.UtcNow;
                    repo.LastUpdated = repo.LastSeen;
                    _store.AddOrUpdate(repo);
                }

                return;
            }
            catch (Exception ex) when (attempt < MAX_RETRIES - 1)
            {
                _logger.LogDebug(ex, "Attempt {Attempt} failed to read repository at {Path}, retrying", attempt + 1, path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read repository after change at {Path}", path);
            }
        }
    }

    private int _stalenessCheckRunning;

    public bool IsStalenessCheckRunning => Volatile.Read(ref _stalenessCheckRunning) != 0;

    public void RemoveStaleRepositories()
    {
        if (Interlocked.CompareExchange(ref _stalenessCheckRunning, 1, 0) != 0)
        {
            return;
        }

        try
        {
            var staleKeys = _store.Items
                .Where(repo => !_fileSystem.Directory.Exists(repo.Path))
                .Select(repo => repo.SafePath)
                .ToList();

            foreach (var key in staleKeys)
            {
                _logger.LogInformation("Removing stale repository: {SafePath}", key);
                _store.Remove(key);
            }
        }
        finally
        {
            Interlocked.Exchange(ref _stalenessCheckRunning, 0);
        }
    }

    private int _refreshRunning;

    public async Task RefreshAllAsync(CancellationToken ct = default)
    {
        if (Interlocked.CompareExchange(ref _refreshRunning, 1, 0) != 0)
        {
            _logger.LogDebug("RefreshAll skipped, already in progress");
            return;
        }

        try
        {
            _logger.LogInformation("Refreshing status of all known repositories");

            RepositoryInfo[] repos = _store.Items
                .Where(r => _monitoringState.IsMonitored(r.SafePath))
                .ToArray();
            if (repos.Length == 0)
            {
                return;
            }

            // Collect all results first, then apply as a single batch to avoid
            // flooding the UI thread with individual DynamicData change notifications.
            var bag = new System.Collections.Concurrent.ConcurrentBag<RepositoryInfo>();

            await Parallel.ForEachAsync(
                repos,
                ct,
                async (repo, token) =>
                {
                    try
                    {
                        // Primary: legacy behavior read ".git/HEAD".
                        // This keeps the existing contract stable and avoids breaking tests.
                        var headPath = _fileSystem.Path.Combine(repo.Path, ".git", "HEAD");
                        RepositoryInfo? updated = await _reader.ReadAsync(headPath, token).ConfigureAwait(false);

                        // Fallback: for worktrees / bare repos, reading ".git/HEAD" may fail
                        // (e.g. when ".git" is a file pointing to a gitdir elsewhere).
                        updated ??= await _reader.ReadAsync(repo.Path, token).ConfigureAwait(false);
                        if (updated == null)
                        {
                            return;
                        }

                        updated.LastSeen = DateTimeOffset.UtcNow;
                        updated.LastUpdated = DateTimeOffset.UtcNow;
                        bag.Add(updated);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to refresh repository {Path}", repo.Path);
                    }
                }).ConfigureAwait(false);

            if (!bag.IsEmpty)
            {
                _store.AddOrUpdateRange(bag);
            }

            _logger.LogInformation("Refreshed {Count} repositories", repos.Length);
        }
        finally
        {
            Interlocked.Exchange(ref _refreshRunning, 0);
        }
    }

    private static readonly TimeSpan _refreshThreshold = TimeSpan.FromSeconds(2);

    public async Task<RepositoryInfo?> RefreshRepositoryAsync(string repositoryPath, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(repositoryPath);

        try
        {
            // If the store already has a recent snapshot, skip the expensive
            // LibGit2Sharp read entirely.  The file-system watcher keeps the
            // store up-to-date in near real-time, so a short freshness window
            // is safe and avoids RetrieveStatus() on every context-menu open.
            var safePath = NormalizeToSafePath(repositoryPath);
            var existing = _store.Lookup(safePath);
            if (existing.HasValue && (DateTimeOffset.UtcNow - existing.Value.LastUpdated) < _refreshThreshold)
            {
                return existing.Value;
            }

            RepositoryInfo? updated = await _reader.ReadAsync(repositoryPath, ct).ConfigureAwait(false);
            if (updated == null)
            {
                return null;
            }

            updated.LastSeen = DateTimeOffset.UtcNow;
            updated.LastUpdated = updated.LastSeen;
            _store.AddOrUpdate(updated);
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to refresh repository {Path}", repositoryPath);
            return null;
        }
    }

    private static string NormalizeToSafePath(string path)
    {
        // Extract repo root from paths like C:\repos\MyRepo\.git\HEAD
        var gitIndex = path.IndexOf(".git", StringComparison.OrdinalIgnoreCase);
        if (gitIndex > 0)
        {
            path = path[..gitIndex].TrimEnd('\\', '/');
        }

        // Single allocation: replace backslashes and trim trailing slash in one pass
        ReadOnlySpan<char> span = path.AsSpan().TrimEnd('/');
        return string.Create(span.Length, span, static (dest, src) =>
        {
            for (var i = 0; i < src.Length; i++)
            {
                dest[i] = src[i] == '\\' ? '/' : src[i];
            }
        });
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _subscriptions?.Dispose();
    }
}
