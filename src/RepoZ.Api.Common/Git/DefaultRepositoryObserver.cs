namespace RepoZ.Api.Common.Git;

using RepoZ.Api.Git;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public sealed class DefaultRepositoryObserver : IRepositoryObserver
{
    private Repository? _repository;
    private FileSystemWatcher? _watcher;
    private bool _ioDetected;

    public Action<Repository> OnChange { get; set; } = delegate { };

    public int DetectionToAlertDelayMilliseconds { get; private set; }

    public void Setup(Repository repository, int detectionToAlertDelayMilliseconds)
    {
        DetectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

        _repository = repository;

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
        PauseWatcherAndScheduleCallback();
    }

    private void WatcherRenamed(object sender, RenamedEventArgs e)
    {
        PauseWatcherAndScheduleCallback();
    }

    private void WatcherChanged(object sender, FileSystemEventArgs e)
    {
        PauseWatcherAndScheduleCallback();
    }

    private void WatcherCreated(object sender, FileSystemEventArgs e)
    {
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
                            
                    Console.WriteLine($"ONCHANGE on {repo.Name}");
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
    }
}