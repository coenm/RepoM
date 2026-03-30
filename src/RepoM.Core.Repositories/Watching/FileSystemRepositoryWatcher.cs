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

            // We pass gitdir paths to this watcher. Git status relevant changes can come from:
            // - HEAD + logs/HEAD (local branch switch)
            // - packed-refs (packed ref updates)
            // - FETCH_HEAD / ORIG_HEAD (fetch/rebase related)
            // - loose refs in refs/** (remote tracking updates when refs are not packed)
            //
            // To avoid a performance-killer, we use multiple FileSystemWatchers with narrow filters
            // rather than "*" over the whole gitdir (which would also include objects/** churn).
            var gitDir = path;

            var headWatcher = CreateWatcherInternal(gitDir, filter: "HEAD", includeSubdirectories: true);
            var packedRefsWatcher = CreateWatcherInternal(gitDir, filter: "packed-refs", includeSubdirectories: false);
            var fetchHeadWatcher = CreateWatcherInternal(gitDir, filter: "FETCH_HEAD", includeSubdirectories: false);
            var origHeadWatcher = CreateWatcherInternal(gitDir, filter: "ORIG_HEAD", includeSubdirectories: false);

            var refsDir = _fileSystem.Path.Combine(gitDir, "refs");
            var refsWatcher = _fileSystem.Directory.Exists(refsDir)
                ? CreateWatcherInternal(refsDir, filter: "*", includeSubdirectories: true)
                : null;

            IObservable<RepositoryChangeEvent> CombineEvents(IFileSystemWatcher? watcher)
            {
                if (watcher is null)
                {
                    return Observable.Empty<RepositoryChangeEvent>();
                }

                var created = Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Created))
                    .Where(e => IsRelevantGitPath(e.EventArgs.FullPath))
                    .Select(e => new RepositoryChangeEvent(e.EventArgs.FullPath, RepositoryChangeType.Added));

                var changed = Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Changed))
                    .Where(e => IsRelevantGitPath(e.EventArgs.FullPath))
                    .Select(e => new RepositoryChangeEvent(e.EventArgs.FullPath, RepositoryChangeType.Modified));

                var deleted = Observable.FromEventPattern<FileSystemEventArgs>(watcher, nameof(watcher.Deleted))
                    .Where(e => IsRelevantGitPath(e.EventArgs.FullPath))
                    .Select(e =>
                    {
                        var fullPath = e.EventArgs.FullPath;
                        // Keep legacy semantics for HEAD deletion: treat it as "Removed".
                        // For other git-metadata deletions (packed-refs / refs/*), we only
                        // mark as Modified to avoid false repo-removals.
                        var isHead = fullPath.EndsWith("HEAD", StringComparison.OrdinalIgnoreCase);
                        return new RepositoryChangeEvent(
                            fullPath,
                            isHead ? RepositoryChangeType.Removed : RepositoryChangeType.Modified);
                    });

                var renamed = Observable.FromEventPattern<RenamedEventArgs>(watcher, nameof(watcher.Renamed))
                    .Where(e => IsRelevantGitPath(e.EventArgs.FullPath))
                    .Select(e => new RepositoryChangeEvent(e.EventArgs.FullPath, RepositoryChangeType.Modified));

                return created
                    .Merge(changed)
                    .Merge(deleted)
                    .Merge(renamed);
            }

            var allEvents = CombineEvents(headWatcher)
                .Merge(CombineEvents(packedRefsWatcher))
                .Merge(CombineEvents(fetchHeadWatcher))
                .Merge(CombineEvents(origHeadWatcher))
                .Merge(CombineEvents(refsWatcher));

            var subscription = allEvents
                .GroupBy(e => NormalizePath(e.Path))
                .SelectMany(group => group.Throttle(_debounceInterval))
                .Subscribe(observer);

            disposables.Add(subscription);

            headWatcher.EnableRaisingEvents = true;
            packedRefsWatcher.EnableRaisingEvents = true;
            fetchHeadWatcher.EnableRaisingEvents = true;
            origHeadWatcher.EnableRaisingEvents = true;
            if (refsWatcher != null)
            {
                refsWatcher.EnableRaisingEvents = true;
            }

            return Disposable.Create(() =>
            {
                headWatcher.EnableRaisingEvents = false;
                packedRefsWatcher.EnableRaisingEvents = false;
                fetchHeadWatcher.EnableRaisingEvents = false;
                origHeadWatcher.EnableRaisingEvents = false;
                if (refsWatcher != null)
                {
                    refsWatcher.EnableRaisingEvents = false;
                }

                headWatcher.Dispose();
                packedRefsWatcher.Dispose();
                fetchHeadWatcher.Dispose();
                origHeadWatcher.Dispose();
                refsWatcher?.Dispose();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create file system watcher for: {Path}", path);
            return null;
        }
    }

    private IFileSystemWatcher CreateWatcherInternal(string directory, string filter, bool includeSubdirectories)
    {
        var watcher = _fileSystem.FileSystemWatcher.New(directory);
        watcher.Filter = filter;
        watcher.IncludeSubdirectories = includeSubdirectories;
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName;
        return watcher;
    }

    private static bool IsRelevantGitPath(string path)
    {
        // We're interested in files that influence "branch / ahead-behind / remote tracking" status.
        // - HEAD / logs/HEAD
        // - packed-refs
        // - FETCH_HEAD / ORIG_HEAD
        // - Loose refs under refs/** (remote tracking refs when not packed)
        //
        // Note: we pass gitdir directories, so paths should already be "mostly relevant",
        // but we keep the filter to reduce Rx/event work.
        var normalized = path;
        return normalized.Contains(".git", StringComparison.OrdinalIgnoreCase)
            && (
                normalized.EndsWith("HEAD", StringComparison.OrdinalIgnoreCase)
                || normalized.EndsWith("packed-refs", StringComparison.OrdinalIgnoreCase)
                || normalized.EndsWith("FETCH_HEAD", StringComparison.OrdinalIgnoreCase)
                || normalized.EndsWith("ORIG_HEAD", StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("\\refs\\", StringComparison.OrdinalIgnoreCase)
                || normalized.Contains("/refs/", StringComparison.OrdinalIgnoreCase)
            );
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
