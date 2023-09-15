namespace RepoM.Api.Git;

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

public sealed class DefaultRepositoryObserver : IRepositoryObserver
{
    private readonly ILogger _logger;
    private int _detectionToAlertDelayMilliseconds;
    private Repository? _repository;
    private FileSystemWatcher? _watcher;
    private bool _ioDetected;
    private LibGit2Sharp.Repository? _gitRepo;

    public Action<Repository> OnChange { get; set; } = delegate { };

    public DefaultRepositoryObserver(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public void Setup(Repository repository, int detectionToAlertDelayMilliseconds)
    {
        _detectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

        _repository = repository;
        try
        {
            _gitRepo = new LibGit2Sharp.Repository(repository.Path);
        }
        catch (Exception)
        {
            _logger.LogWarning("Could not create LibGit2Sharp Repository from path {path}", repository.Path);
        }

        _watcher = new FileSystemWatcher(_repository.Path);
        _watcher.Created += WatcherChangedCreatedOrDeleted;
        _watcher.Changed += WatcherChangedCreatedOrDeleted;
        _watcher.Deleted += WatcherChangedCreatedOrDeleted;
        _watcher.Renamed += WatcherRenamed;
        _watcher.IncludeSubdirectories = true;
    }

    public void Start()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = true;
        }
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
        }
    }

    private bool IsIgnored(FileSystemEventArgs fileSystemEventArgs)
    {
        try
        {
            if (_gitRepo == null)
            {
                return false;
            }

            string? name = fileSystemEventArgs.Name;
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (name.StartsWith(".git", StringComparison.InvariantCulture))
            {
                return false;
            }

            name = name.Replace('\\', '/');

            if (_gitRepo.Ignore.IsPathIgnored(name))
            {
                return true;
            }

            // handle directories, it makes no sense to check if directory exists because it might have been deleted.
            if (_gitRepo.Ignore.IsPathIgnored($"{name}/"))
            {
                return true;
            }

            return false;

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not determine ignored. {message}", e.Message);
            return false;
        }
    }

    private void LogTrace(FileSystemEventArgs fileSystemEventArgs, [CallerMemberName] string caller = "")
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("[DefaultRepositoryObserver] {caller} ({name} - {changeType} - {path})", caller, fileSystemEventArgs.Name, fileSystemEventArgs.ChangeType, fileSystemEventArgs.FullPath);
        }
    }

    private void WatcherRenamed(object sender, RenamedEventArgs e)
    {
        if (IsIgnored(e))
        {
            return;
        }
        LogTrace(e);
        PauseWatcherAndScheduleCallback();
    }

    private void WatcherChangedCreatedOrDeleted(object sender, FileSystemEventArgs e)
    {
        if (IsIgnored(e))
        {
            return;
        }
        LogTrace(e);
        PauseWatcherAndScheduleCallback();
    }

    private void PauseWatcherAndScheduleCallback()
    {
        if (_ioDetected)
        {
            return;
        }

        _ioDetected = true;

        // stop the watcher once we found IO ...
        Stop();

        // ... and schedule a method to reactivate the watchers again
        // if nothing happened in between (regarding IO) it should also fire the OnChange-event
        Task.Run(() => Thread.Sleep(_detectionToAlertDelayMilliseconds))
            .ContinueWith(AwakeWatcherAndScheduleEventInvocationIfNoFurtherIoGetsDetected);
    }

    private void AwakeWatcherAndScheduleEventInvocationIfNoFurtherIoGetsDetected(object state)
    {
        if (!_ioDetected)
        {
            return;
        }

        // reset the flag, wait for further IO ...
        _ioDetected = false;
        Start();

        // ... and if nothing happened during the delay, invoke the OnChange-event
        Task.Run(() => Thread.Sleep(_detectionToAlertDelayMilliseconds))
            .ContinueWith(t =>
                {
                    if (_ioDetected)
                    {
                        return;
                    }

                    Repository? repo = _repository;
                    if (repo == null)
                    {
                        return;
                    }

                    _logger.LogDebug($"ONCHANGE on {repo.Name}");
                    OnChange?.Invoke(repo);
                });
    }

    public void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.Created -= WatcherChangedCreatedOrDeleted;
            _watcher.Changed -= WatcherChangedCreatedOrDeleted;
            _watcher.Deleted -= WatcherChangedCreatedOrDeleted;
            _watcher.Renamed -= WatcherRenamed;
            _watcher.Dispose();
            _watcher = null;
        }

        LibGit2Sharp.Repository? gr = _gitRepo;
        _gitRepo = null;
        gr?.Dispose();
        
    }
}