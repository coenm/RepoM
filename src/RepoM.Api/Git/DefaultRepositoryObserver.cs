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
    private Repository? _repository;
    private FileSystemWatcher? _watcher;
    private bool _ioDetected;
    private LibGit2Sharp.Repository? _gitRepo;

    public Action<Repository> OnChange { get; set; } = delegate { };

    public DefaultRepositoryObserver(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int DetectionToAlertDelayMilliseconds { get; private set; }
    
    public void Setup(Repository repository, int detectionToAlertDelayMilliseconds)
    {
        DetectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

        _repository = repository;
        _gitRepo = new LibGit2Sharp.Repository(repository.Path);

        _watcher = new FileSystemWatcher(_repository.Path);
        _watcher.Created += WatcherCreated;
        _watcher.Changed += WatcherChanged;
        _watcher.Deleted += WatcherDeleted;
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

    private void WatcherDeleted(object sender, FileSystemEventArgs e)
    {
        if (IsIgnored(e))
        {
            return;
        }
        LogTrace(e);
        PauseWatcherAndScheduleCallback();
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

            name = name.Replace('\\', '/');

            bool? result = _gitRepo?.Ignore.IsPathIgnored(name) ?? null;

            if (result == null)
            {
                return false;
            }

            if (result.Value)
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

    private void WatcherChanged(object sender, FileSystemEventArgs e)
    {
        if (IsIgnored(e))
        {
            return;
        }
        LogTrace(e);
        PauseWatcherAndScheduleCallback();
    }

    private void WatcherCreated(object sender, FileSystemEventArgs e)
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
        Task.Run(() => Thread.Sleep(DetectionToAlertDelayMilliseconds))
            .ContinueWith(AwakeWatcherAndScheduleEventInvocationIfNoFurtherIOGetsDetected);
    }

    private void AwakeWatcherAndScheduleEventInvocationIfNoFurtherIOGetsDetected(object state)
    {
        if (!_ioDetected)
        {
            return;
        }

        // reset the flag, wait for further IO ...
        _ioDetected = false;
        Start();

        // ... and if nothing happened during the delay, invoke the OnChange-event
        Task.Run(() => Thread.Sleep(DetectionToAlertDelayMilliseconds))
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
            _watcher.Created -= WatcherCreated;
            _watcher.Changed -= WatcherChanged;
            _watcher.Deleted -= WatcherDeleted;
            _watcher.Renamed -= WatcherRenamed;
            _watcher.Dispose();
            _watcher = null;
        }

        var gr = _gitRepo;
        _gitRepo = null;
        gr?.Dispose();
        
    }
}