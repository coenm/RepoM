namespace RepoM.Api.Git;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git.AutoFetch;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFinder;

public class DefaultRepositoryMonitor : IRepositoryMonitor
{
    public event EventHandler<IRepository>? OnChangeDetected;
    public event EventHandler<string>? OnDeletionDetected;
    public event EventHandler<bool>? OnScanStateChanged;

    private List<IRepositoryDetector>? _detectors;
    private readonly Timer _storeFlushTimer;
    private readonly IRepositoryDetectorFactory _repositoryDetectorFactory;
    private readonly IRepositoryObserverFactory _repositoryObserverFactory;
    private readonly IGitRepositoryFinderFactory _gitRepositoryFinderFactory;
    private readonly IRepositoryReader _repositoryReader;
    private readonly IPathProvider _pathProvider;
    private readonly IRepositoryStore _repositoryStore;
    private readonly IRepositoryInformationAggregator _repositoryInformationAggregator;
    private readonly IRepositoryIgnoreStore _repositoryIgnoreStore;
    private readonly Dictionary<string, IRepositoryObserver> _repositoryObservers;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IAutoFetchHandler _autoFetchHandler;

    private readonly ConcurrentDictionary<string, bool> _pinned = new();

    public DefaultRepositoryMonitor(
        IPathProvider pathProvider,
        IRepositoryReader repositoryReader,
        IRepositoryDetectorFactory repositoryDetectorFactory,
        IRepositoryObserverFactory repositoryObserverFactory,
        IGitRepositoryFinderFactory gitRepositoryFinderFactory,
        IRepositoryStore repositoryStore,
        IRepositoryInformationAggregator repositoryInformationAggregator,
        IAutoFetchHandler autoFetchHandler,
        IRepositoryIgnoreStore repositoryIgnoreStore,
        IFileSystem fileSystem,
        ILogger logger)
    {
        _repositoryReader = repositoryReader ?? throw new ArgumentNullException(nameof(repositoryReader));
        _repositoryDetectorFactory = repositoryDetectorFactory ?? throw new ArgumentNullException(nameof(repositoryDetectorFactory));
        _repositoryObserverFactory = repositoryObserverFactory ?? throw new ArgumentNullException(nameof(repositoryObserverFactory));
        _gitRepositoryFinderFactory = gitRepositoryFinderFactory ?? throw new ArgumentNullException(nameof(gitRepositoryFinderFactory));
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _repositoryStore = repositoryStore ?? throw new ArgumentNullException(nameof(repositoryStore));
        _repositoryInformationAggregator = repositoryInformationAggregator ?? throw new ArgumentNullException(nameof(repositoryInformationAggregator));
        _repositoryIgnoreStore = repositoryIgnoreStore ?? throw new ArgumentNullException(nameof(repositoryIgnoreStore));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _autoFetchHandler = autoFetchHandler ?? throw new ArgumentNullException(nameof(autoFetchHandler));
        _repositoryObservers = new Dictionary<string, IRepositoryObserver>();
        _storeFlushTimer = new Timer(RepositoryStoreFlushTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
    }

    public Task ScanForLocalRepositoriesAsync()
    {
        _logger.LogDebug("{Method} - repo scanning", nameof(ScanForLocalRepositoriesAsync));
        Scanning = true;
        OnScanStateChanged?.Invoke(this, Scanning);

        var scannedPaths = 0;

        var paths = _pathProvider.GetPaths();
        IGitRepositoryFinder gitRepositoryFinder = _gitRepositoryFinderFactory.Create();

        IEnumerable<Task> tasks = paths.Select(path => Task.Run(() => gitRepositoryFinder.Find(path, OnFoundNewRepository))
                           .ContinueWith(_ =>
                               {
                                   if (Interlocked.Increment(ref scannedPaths) != paths.Length)
                                   {
                                       return;
                                   }

                                   Scanning = false;
                                   OnScanStateChanged?.Invoke(this, Scanning);
                               }));

        return Task.WhenAll(tasks);
    }

    private void ScanRepositoriesFromStoreAsync()
    {
        Task.Run(() =>
            {
                foreach (var head in _repositoryStore.Get())
                {
                    _logger.LogDebug("{Method} - repo {Head}", nameof(ScanRepositoriesFromStoreAsync), head);
                    _ = OnCheckKnownRepository(head, KnownRepositoryNotifications.WhenFound);
                }
            });
    }

    private void RepositoryStoreFlushTimerCallback(object? state)
    {
        var heads = _repositoryInformationAggregator.Repositories.Select(v => v.Path).ToArray();
        _repositoryStore.Set(heads);
    }

    private async void OnFoundNewRepository(string file)
    {
        _logger.LogDebug("{Method} - repo {File}", nameof(OnFoundNewRepository), file);

        Repository? repo = null;
        try
        {
            repo = await _repositoryReader.ReadRepositoryAsync(file);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Something went wrong while reading repo.");
        }

        if (repo?.WasFound ?? false)
        {
            OnRepositoryChangeDetected(repo);
        }
    }

    private async Task OnCheckKnownRepository(string file, KnownRepositoryNotifications notification)
    {
        _logger.LogDebug("{Method} - start {Head}", nameof(OnCheckKnownRepository), file);

        Repository? repo = await _repositoryReader.ReadRepositoryAsync(file);
        if (repo?.WasFound ?? false)
        {
            if (notification.HasFlag(KnownRepositoryNotifications.WhenFound))
            {
                OnRepositoryChangeDetected(repo);
            }
        }
        else
        {
            if (notification.HasFlag(KnownRepositoryNotifications.WhenNotFound))
            {
                OnRepositoryDeletionDetected(file);
            }
        }
    }

    private void ObserveRepositoryChanges()
    {
        _logger.LogTrace("ObserveRepositoryChanges start");

        _detectors = new List<IRepositoryDetector>();

        foreach (var path in _pathProvider.GetPaths())
        {
            if (!_fileSystem.Directory.Exists(path))
            {
                continue;
            }

            IRepositoryDetector detector = _repositoryDetectorFactory.Create();
            _detectors.Add(detector);

            detector.OnAddOrChange = OnRepositoryChangeDetected;
            detector.OnDelete = OnRepositoryDeletionDetected;
            detector.Setup(path, DelayGitRepositoryStatusAfterCreationMilliseconds);
        }

        _logger.LogTrace("ObserveRepositoryChanges finished");
    }

    public void Observe()
    {
        _logger.LogTrace("Monitor.Observe starting");
        if (_detectors == null)
        {
            _logger.LogTrace("Monitor.Observe ScanRepositoriesFromStoreAsync");
            ScanRepositoriesFromStoreAsync();

            _logger.LogTrace("Monitor.Observe ObserveRepositoryChanges");
            ObserveRepositoryChanges();
        }

        _logger.LogTrace("Monitor.Observe starting detectors");
        _detectors?.ForEach(w => w.Start());
        _logger.LogTrace("Monitor.Observe starting detectors finished");

        _autoFetchHandler.Active = true;

        _logger.LogTrace("Monitor.Observe finished");
    }

    public void Reset()
    {
        Stop();

        foreach (IRepositoryObserver observer in _repositoryObservers.Values)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (observer == null)
            {
                _logger.LogError("Null Observer in _repositoryObservers detected");
                continue;
            }
            observer.Stop();
            observer.Dispose();
        }

        _repositoryObservers.Clear();

        _repositoryInformationAggregator.Reset();
        RepositoryStoreFlushTimerCallback(null);

        Observe();
    }

    public void Stop()
    {
        _autoFetchHandler.Active = false;
        _detectors?.ForEach(w => w.Stop());
    }

    public void IgnoreByPath(string path)
    {
        _repositoryIgnoreStore.IgnoreByPath(path);
        _repositoryInformationAggregator.RemoveByPath(path);
    }

    public void SetPinned(bool newValue, IRepository repository)
    {
        if (newValue && !_pinned.ContainsKey(repository.SafePath))
        {
            _pinned.AddOrUpdate(repository.SafePath, _ => true, (_, _) => true);
            Task.Run(() => OnRepositoryChangeDetected(repository));
        }

        if (!newValue && _pinned.ContainsKey(repository.SafePath))
        {
            _pinned.TryRemove(repository.SafePath, out var _);
            Task.Run(() => OnRepositoryChangeDetected(repository));
        }
    }

    public bool IsPinned(IRepository repository)
    {
        return _pinned.ContainsKey(repository.SafePath);
    }

    private void CreateRepositoryObserver(IRepository repo, string path)
    {
        if (_repositoryObservers.TryGetValue(path, out IRepositoryObserver? observer))
        {
            return;
        }

        observer = _repositoryObserverFactory.Create();

        if (!_repositoryObservers.TryAdd(path, observer))
        {
            // TODO NOT THREAD SAFE
            observer.Dispose();
            return;
        }

        observer.Setup(repo, DelayGitStatusAfterFileOperationMilliseconds);
        observer.OnChange += OnRepositoryObserverChange;
        observer.Start();
    }

    private void OnRepositoryChangeDetected(IRepository repo)
    {
        var path = repo.Path;

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (_repositoryIgnoreStore.IsIgnored(repo.Path))
        {
            return;
        }

        _logger.LogDebug("{Method} - repo {Head}", nameof(OnRepositoryChangeDetected), repo.Path);

        if (!_repositoryInformationAggregator.HasRepository(path))
        {
            _logger.LogDebug("{Method} - create observer {Head}", nameof(OnRepositoryChangeDetected), repo.Path);
            CreateRepositoryObserver(repo, path);

            // use that delay to prevent a lot of sequential writes
            // when a lot of repositories get found in a row
            _storeFlushTimer.Change(5000, Timeout.Infinite);
        }

        OnChangeDetected?.Invoke(this, repo);

        _repositoryInformationAggregator.Add(repo, this);
    }

    private void OnRepositoryObserverChange(IRepository repository)
    {
        _logger.LogDebug("{Method} - repo {Path}", nameof(OnRepositoryObserverChange), repository.Path);
        _ = OnCheckKnownRepository(repository.Path, KnownRepositoryNotifications.WhenFound | KnownRepositoryNotifications.WhenNotFound);
    }

    private void DestroyRepositoryObserver(string path)
    {
        if (!_repositoryObservers.TryGetValue(path, out IRepositoryObserver? observer))
        {
            return;
        }

        if (!_repositoryObservers.Remove(path))
        {
            return;
        }

        observer.Stop();
    }

    private void OnRepositoryDeletionDetected(string repoPath)
    {
        if (string.IsNullOrEmpty(repoPath))
        {
            return;
        }

        _logger.LogDebug("{Method} - repo {Head}", nameof(OnRepositoryDeletionDetected), repoPath);

        if (_repositoryIgnoreStore.IsIgnored(repoPath))
        {
            return;
        }

        DestroyRepositoryObserver(repoPath);

        OnDeletionDetected?.Invoke(this, repoPath);

        _repositoryInformationAggregator.RemoveByPath(repoPath);
    }

    public bool Scanning { get; private set; } = false;

    public int DelayGitRepositoryStatusAfterCreationMilliseconds { get; init; } = 5000;

    public int DelayGitStatusAfterFileOperationMilliseconds { get; init; } = 500;

    [Flags]
    private enum KnownRepositoryNotifications
    {
        WhenFound = 1,
        WhenNotFound = 2,
    }
}
