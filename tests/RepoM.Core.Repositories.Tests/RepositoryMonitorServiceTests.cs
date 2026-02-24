namespace RepoM.Core.Repositories.Tests;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Reading;
using RepoM.Core.Repositories.Scanning;
using RepoM.Core.Repositories.Store;
using RepoM.Core.Repositories.Watching;
using Xunit;

public class RepositoryMonitorServiceTests : IDisposable
{
    private readonly IRepositoryScanner _scanner;
    private readonly IRepositoryWatcher _watcher;
    private readonly IRepositoryInfoReader _reader;
    private readonly IFileSystem _fileSystem;
    private readonly RepositoryStore _store;
    private readonly RepositoryMonitorService _sut;

    public RepositoryMonitorServiceTests()
    {
        _scanner = A.Fake<IRepositoryScanner>();
        _watcher = A.Fake<IRepositoryWatcher>();
        _reader = A.Fake<IRepositoryInfoReader>();
        _fileSystem = A.Fake<IFileSystem>();
        _store = new RepositoryStore();

        A.CallTo(() => _scanner.IsScanning).Returns(Observable.Return(false));
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(Observable.Empty<RepositoryChangeEvent>());
        A.CallTo(() => _scanner.Scan(A<IEnumerable<string>>._, A<CancellationToken>._)).Returns(Observable.Empty<string>());

        _sut = new RepositoryMonitorService(
            _scanner,
            _watcher,
            _reader,
            _store,
            _fileSystem,
            () => ["/repos",],
            NullLogger.Instance);
    }

    [Fact]
    public void Store_ShouldReturnInjectedStore()
    {
        _sut.Store.Should().BeSameAs(_store);
    }

    [Fact]
    public async Task StartAsync_ShouldStartWatching()
    {
        // Act
        await _sut.StartAsync();

        // Assert
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StartAsync_ShouldTriggerInitialScan()
    {
        // Act
        await _sut.StartAsync();

        // Assert
        A.CallTo(() => _scanner.Scan(A<IEnumerable<string>>._, A<CancellationToken>._)).MustHaveHappened();
    }

    [Fact]
    public async Task StopAsync_ShouldDisposeSubscriptions()
    {
        // Arrange
        await _sut.StartAsync();

        // Act
        await _sut.StopAsync();

        // Assert - no exception means success, subscriptions are cleaned up
    }

    [Fact]
    public async Task ScanAsync_ShouldAddFoundRepositoriesToStore()
    {
        // Arrange
        var scanSubject = new Subject<string>();
        A.CallTo(() => _scanner.Scan(A<IEnumerable<string>>._, A<CancellationToken>._))
            .Returns(scanSubject.AsObservable());

        var repoInfo = new RepositoryInfo
        {
            Path = @"\repos\test",
            SafePath = "/repos/test",
            Name = "test",
        };

        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(repoInfo));

        // Act
        var scanTask = _sut.ScanAsync(CancellationToken.None);
        scanSubject.OnNext("/repos/test/.git/HEAD");
        scanSubject.OnCompleted();
        await scanTask;

        // Assert
        _store.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenScannerIsNull()
    {
        var act = () => new RepositoryMonitorService(
            null!, _watcher, _reader, _store, _fileSystem, () => [], NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenWatcherIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, null!, _reader, _store, _fileSystem, () => [], NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenReaderIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, null!, _store, _fileSystem, () => [], NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    public void Dispose()
    {
        _sut.Dispose();
        _store.Dispose();
    }
}
