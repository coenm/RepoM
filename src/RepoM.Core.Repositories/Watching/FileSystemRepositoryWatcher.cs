namespace RepoM.Core.Repositories.Watching;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Core.Repositories.Model;

public sealed class FileSystemRepositoryWatcher : IRepositoryWatcher
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly TimeSpan _debounceInterval;
    private bool _disposed;

    public FileSystemRepositoryWatcher(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _debounceInterval = TimeSpan.FromMilliseconds(500);
    }

    public IObservable<RepositoryChangeEvent> Watch(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        return Observable.Create<RepositoryChangeEvent>(observer =>
        {
            var disposables = new CompositeDisposable();

            foreach (var path in paths)
            {
                if (!_fileSystem.Directory.Exists(path))
                {
                    _logger.LogWarning("Watch path does not exist: {Path}", path);
                    continue;
                }

                var watcher = CreateWatcher(path, observer, disposables);
                if (watcher != null)
                {
                    disposables.Add(watcher);
                }
            }

            return disposables;
        });
    }

    private IDisposable? CreateWatcher(string path, IObserver<RepositoryChangeEvent> observer, CompositeDisposable disposables)
    {
        try
        {
            _logger.LogDebug("Setting up file system watcher for: {Path}", path);

            var watcher = _fileSystem.FileSystemWatcher.New(path);
            watcher.Filter = "HEAD";
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;

            var created = Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Created))
                .Where(e => IsRelevantGitPath(e.EventArgs.FullPath))
                .Select(e => new RepositoryChangeEvent(e.EventArgs.FullPath, RepositoryChangeType.Added));

            var changed = Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Changed))
                .Where(e => IsRelevantGitPath(e.EventArgs.FullPath))
                .Select(e => new RepositoryChangeEvent(e.EventArgs.FullPath, RepositoryChangeType.Modified));

            var deleted = Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Deleted))
                .Where(e => IsRelevantGitPath(e.EventArgs.FullPath))
                .Select(e => new RepositoryChangeEvent(e.EventArgs.FullPath, RepositoryChangeType.Removed));

            var renamed = Observable.FromEventPattern<RenamedEventArgs>(watcher, nameof(watcher.Renamed))
                .Where(e => IsRelevantGitPath(e.EventArgs.FullPath))
                .Select(e => new RepositoryChangeEvent(e.EventArgs.FullPath, RepositoryChangeType.Modified));

            var subscription = created
                .Merge(changed)
                .Merge(deleted)
                .Merge(renamed)
                .GroupBy(e => NormalizePath(e.Path))
                .SelectMany(group => group.Throttle(_debounceInterval))
                .Subscribe(observer);

            disposables.Add(subscription);

            watcher.EnableRaisingEvents = true;

            return Disposable.Create(() =>
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create file system watcher for: {Path}", path);
            return null;
        }
    }

    private static bool IsRelevantGitPath(string path)
    {
        // We're interested in HEAD files inside .git directories
        // e.g. /path/to/repo/.git/logs/HEAD or /path/to/repo/.git/HEAD
        return path.Contains(".git", StringComparison.OrdinalIgnoreCase)
            && path.EndsWith("HEAD", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        // Extract the repository root from a git HEAD path
        var gitIndex = path.IndexOf(".git", StringComparison.OrdinalIgnoreCase);
        if (gitIndex > 0)
        {
            return path[..gitIndex].TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
        }

        return path;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }
}
