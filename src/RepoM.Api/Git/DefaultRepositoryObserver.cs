namespace RepoM.Api.Git;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public sealed class DefaultRepositoryObserver : IRepositoryObserver
{
    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystem;
    private int _detectionToAlertDelayMilliseconds;
    private Repository? _repository;
    private IFileSystemWatcher? _watcher;
    private bool _ioDetected;
    private LibGit2Sharp.Repository? _gitRepo;

    public Action<Repository> OnChange { get; set; } = delegate { };

    public DefaultRepositoryObserver(ILogger logger, IFileSystem fileSystem)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }
    
    public void Setup(Repository repository, int detectionToAlertDelayMilliseconds)
    {
        _detectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

        _repository = repository;
        try
        {
            _gitRepo = new LibGit2Sharp.Repository(repository.Path);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Could not create LibGit2Sharp Repository from path {Path}", repository.Path);
        }

        
        _watcher = _fileSystem.FileSystemWatcher.New(_repository.Path);
        _watcher.Created += FileSystemUpdated;
        _watcher.Changed += FileSystemUpdated;
        _watcher.Deleted += FileSystemUpdated;
        _watcher.Renamed += FileSystemUpdated;
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

    public void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.Created -= FileSystemUpdated;
            _watcher.Changed -= FileSystemUpdated;
            _watcher.Deleted -= FileSystemUpdated;
            _watcher.Renamed -= FileSystemUpdated;
            _watcher.Dispose();
            _watcher = null;
        }

        LibGit2Sharp.Repository? gr = _gitRepo;
        _gitRepo = null;
        gr?.Dispose();
    }

    private bool IsIgnored(FileSystemEventArgs fileSystemEventArgs)
    {
        if (_gitRepo == null)
        {
            return false;
        }

        var name = fileSystemEventArgs.Name;
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (name.Equals(".git\\index.lock", StringComparison.InvariantCulture))
        {
            return true;
        }

        if (name.StartsWith(".git", StringComparison.InvariantCulture))
        {
            return false;
        }

        name = name.Replace('\\', '/');

        try
        {
            if (_gitRepo.Ignore.IsPathIgnored($"{name}/"))
            {
                return true;
            }

            // when it is a file, check if it is ignored
            if (_gitRepo.Ignore.IsPathIgnored(name))
            {
                return true;
            }

            if (fileSystemEventArgs.ChangeType == WatcherChangeTypes.Changed)
            {
                try
                {
                    if (Directory.Exists(fileSystemEventArgs.FullPath))
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Directory exists failed {Dir} {Message}", fileSystemEventArgs.FullPath, e.Message);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not determine ignored. {Message}", e.Message);
        }


        return false;
    }

    private void FileSystemUpdated(object sender, FileSystemEventArgs e)
    {
        if (IsIgnored(e))
        {
            return;
        }

        _logger.LogTrace("[DefaultRepositoryObserver] {Caller} ({Name} - {ChangeType} - {Path})", "", e.Name, e.ChangeType, e.FullPath);
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
            .ContinueWith(_ =>
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

                    _logger.LogDebug("ONCHANGE on {Repository}", repo.Name);
                    OnChange.Invoke(repo);
                });
    }
}