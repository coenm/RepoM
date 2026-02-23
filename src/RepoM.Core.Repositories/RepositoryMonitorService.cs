namespace RepoM.Core.Repositories;

using System;
using System.Collections.Generic;
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
        Func<IEnumerable<string>> pathProvider,
        ILogger logger)
    {
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _store = store ?? throw new ArgumentNullException(nameof(store));
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
            .Do(repo =>
            {
                repo.LastSeen = DateTimeOffset.UtcNow;
                _store.AddOrUpdate(repo);
            });
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
