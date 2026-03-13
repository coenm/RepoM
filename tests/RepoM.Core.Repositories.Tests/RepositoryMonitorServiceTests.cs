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
        Func<Task> act = () => _sut.StopAsync();

        // Assert
        await act.Should().NotThrowAsync();
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

    [Fact]
    public void Constructor_ShouldThrow_WhenStoreIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, null!, _fileSystem, () => [], NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFileSystemIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, null!, () => [], NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPathProviderIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, null!, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, () => [], null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelAllScans_ShouldNotThrow_WhenCalledBeforeStart()
    {
        var act = () => _sut.CancelAllScans();
        act.Should().NotThrow();
    }

    [Fact]
    public void CancelAllScans_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        var act = () =>
        {
            _sut.CancelAllScans();
            _sut.CancelAllScans();
            _sut.CancelAllScans();
        };
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ScanAsync_ShouldBeCancellable_ViaCancelAllScans()
    {
        // Arrange
        var scanSubject = new Subject<string>();
        A.CallTo(() => _scanner.Scan(A<IEnumerable<string>>._, A<CancellationToken>._))
            .Returns(scanSubject.AsObservable());

        // Act
        var scanTask = _sut.ScanAsync(CancellationToken.None);
        _sut.CancelAllScans();

        // Assert - CancelAllScans may cause TaskCanceledException or complete normally
        Func<Task> act = () => scanTask;
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public void RemoveStaleRepositories_ShouldRemoveNonExistentRepos()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\deleted",
            SafePath = "/repos/deleted",
            Name = "deleted",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\deleted")).Returns(false);

        // Act
        _sut.RemoveStaleRepositories();

        // Assert
        _store.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveStaleRepositories_ShouldKeepExistingRepos()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\existing",
            SafePath = "/repos/existing",
            Name = "existing",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\existing")).Returns(true);

        // Act
        _sut.RemoveStaleRepositories();

        // Assert
        _store.Count.Should().Be(1);
    }

    [Fact]
    public void RemoveStaleRepositories_ShouldBeReentrantSafe()
    {
        // Act - calling twice should not throw
        var act = () =>
        {
            _sut.RemoveStaleRepositories();
            _sut.RemoveStaleRepositories();
        };
        act.Should().NotThrow();
    }

    [Fact]
    public async Task RemoveStaleRepositories_ShouldSkipConcurrentCall()
    {
        // arrange
        var fakeStore = A.Fake<IRepositoryStore>();
        using var called = new ManualResetEventSlim(false);
        using var gate = new ManualResetEventSlim(false);

        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\blocking",
            SafePath = "c:/repos/blocking",
            Name = "blocking",
        };

        A.CallTo(() => fakeStore.Items).Returns([repoInfo,]);
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\blocking"))
            .ReturnsLazily(call =>
            {
                called.Set();
                gate.Wait(); // hold the first call inside RemoveStaleRepositories
                return true;
            });

        using var sut = new RepositoryMonitorService(
            _scanner, _watcher, _reader, fakeStore, _fileSystem, () => ["/repos",], NullLogger.Instance);

        // act
        // Start the first (blocking) call on a background thread
        var firstCall = Task.Run(() => sut.RemoveStaleRepositories());

        // Wait until the first call has entered the method
        called.Wait(TimeSpan.FromMilliseconds(500));
        sut.IsStalenessCheckRunning.Should().BeTrue();

        // The second call should return immediately (early-return branch)
        sut.RemoveStaleRepositories();

        // Release the first call
        gate.Set();
        await firstCall;

        // assert
        sut.IsStalenessCheckRunning.Should().BeFalse();
        A.CallTo(() => fakeStore.Items).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IsStalenessCheckRunning_ShouldBeFalse_Initially()
    {
        _sut.IsStalenessCheckRunning.Should().BeFalse();
    }

    [Fact]
    public void IsScanning_ShouldExposeScannersObservable()
    {
        // Arrange
        var subject = new BehaviorSubject<bool>(true);
        A.CallTo(() => _scanner.IsScanning).Returns(subject.AsObservable());

        var sut = new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, () => ["/repos",], NullLogger.Instance);

        // Act
        var isScanning = sut.IsScanning.FirstAsync().Wait();

        // Assert
        isScanning.Should().BeTrue();

        sut.Dispose();
        subject.Dispose();
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        var act = () =>
        {
            _sut.Dispose();
            _sut.Dispose();
        };
        act.Should().NotThrow();
    }

    [Fact]
    public async Task OnRepositoryChangeDetected_ShouldAddRepository_WhenChangeTypeIsAdded()
    {
        // Arrange
        var watchSubject = new Subject<RepositoryChangeEvent>();
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(watchSubject.AsObservable());

        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\newrepo",
            SafePath = "c:/repos/newrepo",
            Name = "newrepo",
        };
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(repoInfo));

        await _sut.StartAsync();

        // Act
        watchSubject.OnNext(new RepositoryChangeEvent(@"c:\repos\newrepo\.git\HEAD", RepositoryChangeType.Added));

        // Assert - give async pipeline time to complete
        await Task.Delay(500);
        A.CallTo(() => _reader.ReadAsync(@"c:\repos\newrepo\.git\HEAD", A<CancellationToken>._)).MustHaveHappened();
    }

    [Fact]
    public async Task OnRepositoryChangeDetected_ShouldAddRepository_WhenChangeTypeIsModified()
    {
        // Arrange
        var watchSubject = new Subject<RepositoryChangeEvent>();
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(watchSubject.AsObservable());

        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\modifiedrepo",
            SafePath = "c:/repos/modifiedrepo",
            Name = "modifiedrepo",
        };
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(repoInfo));

        await _sut.StartAsync();

        // Act
        watchSubject.OnNext(new RepositoryChangeEvent(@"c:\repos\modifiedrepo\.git\HEAD", RepositoryChangeType.Modified));

        // Assert
        await Task.Delay(500);
        A.CallTo(() => _reader.ReadAsync(@"c:\repos\modifiedrepo\.git\HEAD", A<CancellationToken>._)).MustHaveHappened();
    }

    [Fact]
    public async Task OnRepositoryChangeDetected_ShouldRemoveRepository_WhenChangeTypeIsRemoved()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\removedrepo",
            SafePath = "c:/repos/removedrepo",
            Name = "removedrepo",
        };
        _store.AddOrUpdate(repoInfo);
        _store.Count.Should().Be(1);

        var watchSubject = new Subject<RepositoryChangeEvent>();
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(watchSubject.AsObservable());

        await _sut.StartAsync();

        // Act
        watchSubject.OnNext(new RepositoryChangeEvent(@"c:\repos\removedrepo\.git\HEAD", RepositoryChangeType.Removed));

        // Assert
        await Task.Delay(100);
        _store.Count.Should().Be(0);
    }

    [Fact]
    public async Task ScanAsync_ShouldSkipNullResults()
    {
        // Arrange
        var scanSubject = new Subject<string>();
        A.CallTo(() => _scanner.Scan(A<IEnumerable<string>>._, A<CancellationToken>._))
            .Returns(scanSubject.AsObservable());
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(null));

        // Act
        var scanTask = _sut.ScanAsync(CancellationToken.None);
        scanSubject.OnNext("/repos/test/.git/HEAD");
        scanSubject.OnCompleted();
        await scanTask;

        // Assert
        _store.Count.Should().Be(0);
    }

    [Fact]
    public async Task ScanAsync_ShouldBeCancellable_ViaExternalToken()
    {
        // Arrange
        var scanSubject = new Subject<string>();
        A.CallTo(() => _scanner.Scan(A<IEnumerable<string>>._, A<CancellationToken>._))
            .Returns(scanSubject.AsObservable());

        using var cts = new CancellationTokenSource();

        // Act
        var scanTask = _sut.ScanAsync(cts.Token);
        cts.Cancel();

        // Assert
        Func<Task> act = () => scanTask;
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    [Fact]
    public async Task StopAsync_ShouldNotThrow_WhenCalledWithoutStart()
    {
        // Act
        Func<Task> act = () => _sut.StopAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ScanAsync_ShouldSetLastSeenOnRepository()
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

        var beforeScan = DateTimeOffset.UtcNow;

        // Act
        var scanTask = _sut.ScanAsync(CancellationToken.None);
        scanSubject.OnNext("/repos/test/.git/HEAD");
        scanSubject.OnCompleted();
        await scanTask;

        // Assert
        repoInfo.LastSeen.Should().BeOnOrAfter(beforeScan);
    }

    [Fact]
    public async Task RefreshAllAsync_ShouldReturnImmediately_WhenStoreIsEmpty()
    {
        // Act
        Func<Task> act = () => _sut.RefreshAllAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RefreshAllAsync_ShouldUpdateRepositoriesInStore()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\myrepo",
            SafePath = "c:/repos/myrepo",
            Name = "myrepo",
        };
        _store.AddOrUpdate(repoInfo);

        var updatedRepo = new RepositoryInfo
        {
            Path = @"c:\repos\myrepo",
            SafePath = "c:/repos/myrepo",
            Name = "myrepo",
            CurrentBranch = "feature/xyz",
        };

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\myrepo", ".git", "HEAD"))
            .Returns(@"c:\repos\myrepo\.git\HEAD");
        A.CallTo(() => _reader.ReadAsync(@"c:\repos\myrepo\.git\HEAD", A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(updatedRepo));

        var beforeRefresh = DateTimeOffset.UtcNow;

        // Act
        await _sut.RefreshAllAsync(CancellationToken.None);

        // Assert
        _store.Count.Should().Be(1);
        updatedRepo.LastSeen.Should().BeOnOrAfter(beforeRefresh);
        updatedRepo.LastUpdated.Should().BeOnOrAfter(beforeRefresh);
    }

    [Fact]
    public async Task RefreshAllAsync_ShouldSkipRepos_WhenReaderReturnsNull()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\myrepo",
            SafePath = "c:/repos/myrepo",
            Name = "myrepo",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\myrepo", ".git", "HEAD"))
            .Returns(@"c:\repos\myrepo\.git\HEAD");
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(null));

        // Act
        await _sut.RefreshAllAsync(CancellationToken.None);

        // Assert - original repo remains (no crash, no removal)
        _store.Count.Should().Be(1);
    }

    [Fact]
    public async Task RefreshAllAsync_ShouldHandleExceptions_WhenReaderThrows()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\myrepo",
            SafePath = "c:/repos/myrepo",
            Name = "myrepo",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\myrepo", ".git", "HEAD"))
            .Returns(@"c:\repos\myrepo\.git\HEAD");
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new InvalidOperationException("read failed"));

        // Act
        Func<Task> act = () => _sut.RefreshAllAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RefreshAllAsync_ShouldBeCancellable()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\myrepo",
            SafePath = "c:/repos/myrepo",
            Name = "myrepo",
        };
        _store.AddOrUpdate(repoInfo);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        A.CallTo(() => _fileSystem.Path.Combine(A<string>._, A<string>._, A<string>._))
            .Returns(@"c:\repos\myrepo\.git\HEAD");

        // Act
        Func<Task> act = () => _sut.RefreshAllAsync(cts.Token);

        // Assert - should throw or complete, but not hang
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void RemoveStaleRepositories_ShouldRemoveOnlyStaleRepos_WhenMixed()
    {
        // Arrange
        var staleRepo = new RepositoryInfo
        {
            Path = @"c:\repos\stale",
            SafePath = "c:/repos/stale",
            Name = "stale",
        };
        var existingRepo = new RepositoryInfo
        {
            Path = @"c:\repos\existing2",
            SafePath = "c:/repos/existing2",
            Name = "existing2",
        };
        _store.AddOrUpdate(staleRepo);
        _store.AddOrUpdate(existingRepo);

        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\stale")).Returns(false);
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\existing2")).Returns(true);

        // Act
        _sut.RemoveStaleRepositories();

        // Assert
        _store.Count.Should().Be(1);
        _store.Lookup("c:/repos/existing2").HasValue.Should().BeTrue();
        _store.Lookup("c:/repos/stale").HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task Dispose_ShouldNotThrow_AfterStartAsync()
    {
        // Arrange
        await _sut.StartAsync();

        // Act
        Action act = () => _sut.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task StopAsync_ThenStartAsync_ShouldWork()
    {
        // Arrange
        await _sut.StartAsync();
        await _sut.StopAsync();

        // Act
        Func<Task> act = () => _sut.StartAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _sut.Dispose();
        _store.Dispose();
    }
}
