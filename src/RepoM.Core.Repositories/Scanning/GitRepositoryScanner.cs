namespace RepoM.Core.Repositories.Scanning;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.Extensions.Logging;

public sealed class GitRepositoryScanner : IRepositoryScanner
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly BehaviorSubject<bool> _isScanning = new(false);
    private int _activeScanCount;
    private bool _disposed;

    public GitRepositoryScanner(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IObservable<bool> IsScanning => _isScanning.AsObservable().DistinctUntilChanged();

    public IObservable<string> Scan(IEnumerable<string> paths, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(paths);

        return Observable.Create<string>(observer =>
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            IncrementScanCount();

            try
            {
                foreach (var path in paths)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    ScanPath(path, observer, cts.Token);
                }

                observer.OnCompleted();
            }
            catch (OperationCanceledException)
            {
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during repository scan");
                observer.OnError(ex);
            }
            finally
            {
                DecrementScanCount();
            }

            return Disposable.Create(() => cts.Cancel());
        }).SubscribeOn(System.Reactive.Concurrency.TaskPoolScheduler.Default);
    }

    private void ScanPath(string root, IObserver<string> observer, CancellationToken ct)
    {
        if (!_fileSystem.Directory.Exists(root))
        {
            _logger.LogWarning("Scan path does not exist: {Path}", root);
            return;
        }

        _logger.LogDebug("Scanning for repositories in: {Path}", root);

        var pending = new Queue<string>();
        pending.Enqueue(root);

        while (pending.Count > 0)
        {
            ct.ThrowIfCancellationRequested();

            var current = pending.Dequeue();

            if (IsGitRepository(current))
            {
                var headPath = _fileSystem.Path.Combine(current, ".git", "logs", "HEAD");
                if (_fileSystem.File.Exists(headPath))
                {
                    observer.OnNext(headPath);
                }
                else
                {
                    var gitHeadPath = _fileSystem.Path.Combine(current, ".git", "HEAD");
                    if (_fileSystem.File.Exists(gitHeadPath))
                    {
                        observer.OnNext(gitHeadPath);
                    }
                }

                // Don't descend into subdirectories of a git repo's .git folder,
                // but do allow nested repos (e.g. submodules)
            }

            string[] subdirectories;
            try
            {
                subdirectories = _fileSystem.Directory.GetDirectories(current);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                continue;
            }

            foreach (var subdir in subdirectories)
            {
                var dirName = _fileSystem.Path.GetFileName(subdir);

                // Skip common non-repository directories
                if (ShouldSkipDirectory(dirName))
                {
                    continue;
                }

                pending.Enqueue(subdir);
            }
        }
    }

    private bool IsGitRepository(string path)
    {
        var gitDir = _fileSystem.Path.Combine(path, ".git");
        return _fileSystem.Directory.Exists(gitDir) || _fileSystem.File.Exists(gitDir);
    }

    private static bool ShouldSkipDirectory(string dirName)
    {
        return string.Equals(dirName, ".git", StringComparison.OrdinalIgnoreCase)
            || string.Equals(dirName, "node_modules", StringComparison.OrdinalIgnoreCase)
            || string.Equals(dirName, "bin", StringComparison.OrdinalIgnoreCase)
            || string.Equals(dirName, "obj", StringComparison.OrdinalIgnoreCase)
            || string.Equals(dirName, ".vs", StringComparison.OrdinalIgnoreCase)
            || string.Equals(dirName, "$RECYCLE.BIN", StringComparison.OrdinalIgnoreCase)
            || string.Equals(dirName, "System Volume Information", StringComparison.OrdinalIgnoreCase);
    }

    private void IncrementScanCount()
    {
        if (Interlocked.Increment(ref _activeScanCount) == 1)
        {
            _isScanning.OnNext(true);
        }
    }

    private void DecrementScanCount()
    {
        if (Interlocked.Decrement(ref _activeScanCount) == 0)
        {
            _isScanning.OnNext(false);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _isScanning.OnCompleted();
        _isScanning.Dispose();
    }
}
