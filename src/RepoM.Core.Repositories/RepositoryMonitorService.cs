namespace RepoM.Core.Repositories;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Reading;
using RepoM.Core.Repositories.Scanning;
using RepoM.Core.Repositories.Store;
using RepoM.Core.Repositories.Watching;

public class RepositoryMonitorService : IModule, IDisposable
{
    private readonly IRepositoryScanner _scanner;
    private readonly IRepositoryWatcher _watcher;
    private readonly IRepositoryInfoReader _reader;
    private readonly IRepositoryStore _store;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly Func<IEnumerable<string>> _pathProvider;
    private readonly TimeSpan _scanInterval;
    private readonly Lock _scanLock = new();
    private CompositeDisposable? _subscriptions;
    private CancellationTokenSource _scanCts = new();
    private bool _disposed;

    public RepositoryMonitorService(
        IRepositoryScanner scanner,
        IRepositoryWatcher watcher,
        IRepositoryInfoReader reader,
        IRepositoryStore store,
        IFileSystem fileSystem,
        Func<IEnumerable<string>> pathProvider,
        ILogger logger)
    {
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scanInterval = TimeSpan.FromMinutes(30);
    }

    public IRepositoryStore Store => _store;

    public IObservable<bool> IsScanning => _scanner.IsScanning;

    public Task StartAsync()
    {
        _logger.LogInformation("RepositoryMonitorService starting");

        _subscriptions = new CompositeDisposable();

        // Start filesystem watching for real-time detection
        var watchSubscription = _watcher
            .Watch(_pathProvider())
            .Subscribe(
                OnRepositoryChangeDetected,
                ex => _logger.LogError(ex, "Error in repository watcher"));
        _subscriptions.Add(watchSubscription);

        // Initial scan
        var initialScanSubscription = CreateScanPipeline(_scanCts.Token)
            .Subscribe(
                _ => { },
                ex => _logger.LogError(ex, "Error during initial scan"),
                () => _logger.LogInformation("Initial scan completed"));
        _subscriptions.Add(initialScanSubscription);

        // Periodic scan
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

                    return CreateScanPipeline(token);
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
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _logger.LogInformation("RepositoryMonitorService stopping");

        _subscriptions?.Dispose();
        _subscriptions = null;

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
            .SelectMany(path => Observable.FromAsync(token => _reader.ReadAsync(path, token)))
            .Where(repo => repo != null)
            .Select(repo => repo!)
            .Do(repo => repo.LastSeen = DateTimeOffset.UtcNow)
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
                Observable
                    .FromAsync(ct => _reader.ReadAsync(changeEvent.Path, ct))
                    .Retry(3)
                    .Where(repo => repo != null)
                    .Subscribe(
                        repo =>
                        {
                            repo!.LastSeen = DateTimeOffset.UtcNow;
                            repo.LastUpdated = DateTimeOffset.UtcNow;
                            _store.AddOrUpdate(repo);
                        },
                        ex => _logger.LogWarning(ex, "Failed to read repository after change at {Path}", changeEvent.Path));
                break;

            case RepositoryChangeType.Removed:
                _logger.LogDebug("Repository removal detected at {Path}", changeEvent.Path);
                var safePath = NormalizeToSafePath(changeEvent.Path);
                _store.Remove(safePath);
                break;
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

            RepositoryInfo[] repos = _store.Items.ToArray();
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
                        var headPath = _fileSystem.Path.Combine(repo.Path, ".git", "HEAD");
                        RepositoryInfo? updated = await _reader.ReadAsync(headPath, token).ConfigureAwait(false);
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
                _store.AddOrUpdateRange(bag.ToList());
            }

            _logger.LogInformation("Refreshed {Count} repositories", repos.Length);
        }
        finally
        {
            Interlocked.Exchange(ref _refreshRunning, 0);
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

        return path.Replace('\\', '/').TrimEnd('/');
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
