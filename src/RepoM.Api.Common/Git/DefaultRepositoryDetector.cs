namespace RepoM.Api.Common.Git;

using System;
using System.IO;
using System.Threading.Tasks;
using RepoM.Api.Git;

public sealed class DefaultRepositoryDetector : IRepositoryDetector, IDisposable
{
    private const string HEAD_LOG_FILE = @".git\logs\HEAD";
    private FileSystemWatcher? _watcher;
    private readonly IRepositoryReader _repositoryReader;

    public DefaultRepositoryDetector(IRepositoryReader repositoryReader)
    {
        _repositoryReader = repositoryReader;
    }

    public Action<Repository>? OnAddOrChange { get; set; }

    public Action<string>? OnDelete { get; set; }

    public void Setup(string path, int detectionToAlertDelayMilliseconds)
    {
        DetectionToAlertDelayMilliseconds = detectionToAlertDelayMilliseconds;

        _watcher = new FileSystemWatcher(path);
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
        if (!IsHead(e.FullPath))
        {
            return;
        }

        NotifyHeadDeletion(e.FullPath);
    }

    private void WatcherRenamed(object sender, RenamedEventArgs e)
    {
        if (!IsHead(e.OldFullPath))
        {
            return;
        }

        NotifyHeadDeletion(e.OldFullPath);
    }

    private void WatcherChanged(object sender, FileSystemEventArgs e)
    {
        if (!IsHead(e.FullPath))
        {
            return;
        }

        EatRepo(e.FullPath);
    }

    private void WatcherCreated(object sender, FileSystemEventArgs e)
    {
        if (!IsHead(e.FullPath))
        {
            return;
        }

        Task.Run(() => Task.Delay(DetectionToAlertDelayMilliseconds))
            .ContinueWith(t => EatRepo(e.FullPath));
    }

    private static bool IsHead(string path)
    {
        var index = GetGitPathEndFromHeadFile(path);
        return index == (path.Length - HEAD_LOG_FILE.Length);
    }

    private static string GetRepositoryPathFromHead(string headFile)
    {
        var end = GetGitPathEndFromHeadFile(headFile);

        if (end < 0)
        {
            return string.Empty;
        }

        return headFile.Substring(0, end);
    }

    private static int GetGitPathEndFromHeadFile(string path)
    {
        return path.IndexOf(HEAD_LOG_FILE, StringComparison.OrdinalIgnoreCase);
    }

    private void EatRepo(string path)
    {
        Repository? repo = _repositoryReader.ReadRepository(path);

        if (repo?.WasFound ?? false)
        {
            OnAddOrChange?.Invoke(repo);
        }
    }

    private void NotifyHeadDeletion(string headFile)
    {
        var path = GetRepositoryPathFromHead(headFile);
        if (!string.IsNullOrEmpty(path))
        {
            OnDelete?.Invoke(path);
        }
    }

    public void Dispose()
    {
        if (_watcher == null)
        {
            return;
        }

        _watcher.Created -= WatcherCreated;
        _watcher.Changed -= WatcherChanged;
        _watcher.Deleted -= WatcherDeleted;
        _watcher.Renamed -= WatcherRenamed;
        _watcher.Dispose();
        _watcher = null;
    }

    public int DetectionToAlertDelayMilliseconds { get; private set; }
}