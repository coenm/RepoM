namespace RepoM.Core.Repositories.Scanning;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public sealed class GitRepositoryScanner : IRepositoryScanner
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly BehaviorSubject<bool> _isScanning = new(false);
    private readonly int _degreeOfParallelism;
    private int _activeScanCount;
    private bool _disposed;

    public GitRepositoryScanner(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _degreeOfParallelism = Math.Max(1, Environment.ProcessorCount);
    }

    public IObservable<bool> IsScanning => _isScanning.AsObservable().DistinctUntilChanged();

    public IObservable<string> Scan(IEnumerable<string> paths, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(paths);

        return Observable.Create<string>(observer =>
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // Run the parallel scan on a background thread
            _ = Task.Run(async () =>
            {
                IncrementScanCount();
                try
                {
                    await ScanPathsParallelAsync(paths, observer, cts.Token).ConfigureAwait(false);
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
            }, cts.Token);

            return Disposable.Create(() => cts.Cancel());
        });
    }

    private async Task ScanPathsParallelAsync(IEnumerable<string> roots, IObserver<string> observer, CancellationToken ct)
    {
        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

        // Seed the work queue with all root paths
        var workQueue = new ConcurrentQueue<string>();
        foreach (var root in roots)
        {
            ct.ThrowIfCancellationRequested();

            if (!_fileSystem.Directory.Exists(root))
            {
                _logger.LogWarning("Scan path does not exist: {Path}", root);
                continue;
            }

            _logger.LogDebug("Scanning for repositories in: {Path}", root);
            workQueue.Enqueue(root);
        }

        if (workQueue.IsEmpty)
        {
            return;
        }

        // activeWorkers tracks how many workers are currently processing a directory.
        // When a worker dequeues an item it increments; when done processing it decrements.
        // Completion is detected when all workers are idle AND the queue is empty.
        var activeWorkers = 0;
        var completionSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void SignalIfDone()
        {
            if (Volatile.Read(ref activeWorkers) == 0 && workQueue.IsEmpty)
            {
                completionSignal.TrySetResult();
            }
        }

        var workerCount = Math.Min(_degreeOfParallelism, Math.Max(1, workQueue.Count));
        var workers = new Task[workerCount];

        for (var i = 0; i < workerCount; i++)
        {
            workers[i] = Task.Run(() =>
            {
                var spinWait = new SpinWait();

                while (!ct.IsCancellationRequested)
                {
                    if (workQueue.TryDequeue(out var current))
                    {
                        Interlocked.Increment(ref activeWorkers);
                        try
                        {
                            ProcessDirectory(current, workQueue, channel.Writer, ct);
                        }
                        catch (OperationCanceledException)
                        {
                            // Expected on cancellation — exit gracefully
                        }
                        finally
                        {
                            Interlocked.Decrement(ref activeWorkers);
                            SignalIfDone();
                        }
                    }
                    else
                    {
                        // Queue is empty — check if all workers are idle
                        if (Volatile.Read(ref activeWorkers) == 0 && workQueue.IsEmpty)
                        {
                            completionSignal.TrySetResult();
                            return;
                        }

                        spinWait.SpinOnce();
                    }
                }

                // Cancelled — signal completion so we don't hang
                SignalIfDone();
            }, CancellationToken.None);  // Don't pass ct here — we handle cancellation inside the loop
        }

        // Reader task: forward channel items to the observer (serialized)
        var readerTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var path in channel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
                {
                    observer.OnNext(path);
                }
            }
            catch (OperationCanceledException)
            {
                // Drain remaining items without forwarding
            }
        }, CancellationToken.None);

        // Wait for all workers to finish or cancellation
        await completionSignal.Task.ConfigureAwait(false);
        channel.Writer.Complete();

        // Wait for reader to drain
        await readerTask.ConfigureAwait(false);
    }

    private void ProcessDirectory(string current, ConcurrentQueue<string> workQueue, ChannelWriter<string> writer, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (IsGitRepository(current))
        {
            var headPath = _fileSystem.Path.Combine(current, ".git", "logs", "HEAD");
            if (_fileSystem.File.Exists(headPath))
            {
                writer.TryWrite(headPath);
            }
            else
            {
                var gitHeadPath = _fileSystem.Path.Combine(current, ".git", "HEAD");
                if (_fileSystem.File.Exists(gitHeadPath))
                {
                    writer.TryWrite(gitHeadPath);
                }
            }
        }

        string[] subdirectories;
        try
        {
            subdirectories = _fileSystem.Directory.GetDirectories(current);
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            return;
        }

        foreach (var subdir in subdirectories)
        {
            var dirName = _fileSystem.Path.GetFileName(subdir);

            if (ShouldSkipDirectory(dirName))
            {
                continue;
            }

            workQueue.Enqueue(subdir);
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
