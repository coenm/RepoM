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
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Persistence;
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
    private readonly RepositoryMonitoringStateService _monitoringState;
    private readonly IRepositorySnapshotStore _snapshotStore;
    private readonly RepositoryMonitorService _sut;

    public RepositoryMonitorServiceTests()
    {
        _scanner = A.Fake<IRepositoryScanner>();
        _watcher = A.Fake<IRepositoryWatcher>();
        _reader = A.Fake<IRepositoryInfoReader>();
        _fileSystem = A.Fake<IFileSystem>();
        _store = new RepositoryStore();
        _monitoringState = new RepositoryMonitoringStateService();
        _snapshotStore = A.Fake<IRepositorySnapshotStore>();

        A.CallTo(() => _scanner.IsScanning).Returns(Observable.Return(false));
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(Observable.Empty<RepositoryChangeEvent>());
        A.CallTo(() => _scanner.Scan(A<IEnumerable<string>>._, A<CancellationToken>._)).Returns(Observable.Empty<string>());
        A.CallTo(() => _snapshotStore.LoadAsync(A<CancellationToken>._)).Returns([]);
        A.CallTo(() => _snapshotStore.SaveAsync(A<IEnumerable<RepositoryInfo>>._, A<CancellationToken>._)).Returns(Task.CompletedTask);

        _sut = new RepositoryMonitorService(
            _scanner,
            _watcher,
            _reader,
            _store,
            _fileSystem,
            () => ["/repos",],
            _monitoringState,
            _monitoringState,
            _snapshotStore,
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
    public async Task StartAsync_ShouldLoadRepositoriesFromSnapshot()
    {
        // Arrange
        var snapshotStore = A.Fake<IRepositorySnapshotStore>();
        var repository = new RepositoryInfo { Path = @"c:\repos\cached", SafePath = "c:/repos/cached", Name = "cached", };
        A.CallTo(() => snapshotStore.LoadAsync(A<CancellationToken>._)).Returns([repository,]);
        using var sut = new RepositoryMonitorService(
            _scanner,
            _watcher,
            _reader,
            _store,
            _fileSystem,
            () => ["/repos",],
            _monitoringState,
            _monitoringState,
            snapshotStore,
            NullLogger.Instance);

        // Act
        await sut.StartAsync();

        // Assert
        _store.Lookup("c:/repos/cached").HasValue.Should().BeTrue();
        A.CallTo(() => snapshotStore.LoadAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StartAsync_ShouldSaveSnapshot_WhenRepositoryStoreChanges()
    {
        // Arrange
        var snapshotStore = A.Fake<IRepositorySnapshotStore>();
        var saveCompleted = new TaskCompletionSource();
        A.CallTo(() => snapshotStore.LoadAsync(A<CancellationToken>._)).Returns([]);
        A.CallTo(() => snapshotStore.SaveAsync(A<IEnumerable<RepositoryInfo>>._, A<CancellationToken>._))
            .Invokes(() => saveCompleted.TrySetResult())
            .Returns(Task.CompletedTask);
        using var sut = new RepositoryMonitorService(
            _scanner,
            _watcher,
            _reader,
            _store,
            _fileSystem,
            () => ["/repos",],
            _monitoringState,
            _monitoringState,
            snapshotStore,
            NullLogger.Instance);
        await sut.StartAsync();

        // Act
        _store.AddOrUpdate(new RepositoryInfo { Path = @"c:\repos\saved", SafePath = "c:/repos/saved", Name = "saved", });
        await saveCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Assert
        A.CallTo(() => snapshotStore.SaveAsync(A<IEnumerable<RepositoryInfo>>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
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
            null!, _watcher, _reader, _store, _fileSystem, () => [], _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenWatcherIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, null!, _reader, _store, _fileSystem, () => [], _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenReaderIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, null!, _store, _fileSystem, () => [], _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenStoreIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, null!, _fileSystem, () => [], _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFileSystemIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, null!, () => [], _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenPathProviderIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, null!, _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, () => [], _monitoringState, _monitoringState, _snapshotStore, null!);
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
        using var called = new SemaphoreSlim(0, 1);
        using var gate = new SemaphoreSlim(0, 1);

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
                called.Release();
                gate.Wait(); // hold the first call inside RemoveStaleRepositories
                return true;
            });

        using var sut = new RepositoryMonitorService(
            _scanner, _watcher, _reader, fakeStore, _fileSystem, () => ["/repos",], _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);

        // act
        // Start the first (blocking) call on a background thread
        var firstCall = Task.Run(() => sut.RemoveStaleRepositories());

        // Wait until the first call has entered the method
        var entered = await called.WaitAsync(TimeSpan.FromSeconds(30));
        entered.Should().BeTrue("the first call should have entered RemoveStaleRepositories within the timeout");
        sut.IsStalenessCheckRunning.Should().BeTrue();

        // The second call should return immediately (early-return branch)
        sut.RemoveStaleRepositories();

        // Release the first call
        gate.Release();
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
    public async Task IsScanning_ShouldExposeScannersObservable()
    {
        // Arrange
        var subject = new BehaviorSubject<bool>(true);
        A.CallTo(() => _scanner.IsScanning).Returns(subject.AsObservable());

        var sut = new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, () => ["/repos",], _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);

        // Act
        var isScanning = await sut.IsScanning.FirstAsync();

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
        _monitoringState.SetMonitored("c:/repos/myrepo", true);

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
        _monitoringState.SetMonitored("c:/repos/myrepo", true);

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

    [Fact]
    public void Constructor_ShouldThrow_WhenMonitoringStateIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, () => [], null!, _monitoringState, _snapshotStore, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMonitoringEventsIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, () => [], _monitoringState, null!, _snapshotStore, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSnapshotStoreIsNull()
    {
        var act = () => new RepositoryMonitorService(
            _scanner, _watcher, _reader, _store, _fileSystem, () => [], _monitoringState, _monitoringState, null!, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ActivateMonitoring_ShouldSetMonitoredTrue()
    {
        // Act
        _sut.ActivateMonitoring("c:/repos/myrepo");

        // Assert
        _monitoringState.IsMonitored("c:/repos/myrepo").Should().BeTrue();
    }

    [Fact]
    public void DeactivateMonitoring_ShouldSetMonitoredFalse()
    {
        // Arrange
        _monitoringState.SetMonitored("c:/repos/myrepo", true);

        // Act
        _sut.DeactivateMonitoring("c:/repos/myrepo");

        // Assert
        _monitoringState.IsMonitored("c:/repos/myrepo").Should().BeFalse();
    }

    [Fact]
    public async Task OnMonitoringStateChanged_ShouldAddWatcher_WhenRepoExistsAndMonitored()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\watched",
            SafePath = "c:/repos/watched",
            Name = "watched",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\watched", ".git"))
            .Returns(@"c:\repos\watched\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\watched\.git"))
            .Returns(true);
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._))
            .Returns(Observable.Empty<RepositoryChangeEvent>());

        await _sut.StartAsync();

        // Act — trigger the event handler
        _sut.ActivateMonitoring("c:/repos/watched");

        // Assert — watcher.Watch should have been called for the repo's .git dir
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>.That.Contains(@"c:\repos\watched\.git")))
            .MustHaveHappened();
    }

    [Fact]
    public async Task OnMonitoringStateChanged_ShouldNotThrow_WhenRepoNotInStore()
    {
        // Arrange
        await _sut.StartAsync();

        // Act — activate monitoring for a repo that doesn't exist in the store
        Action act = () => _sut.ActivateMonitoring("c:/repos/nonexistent");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task OnMonitoringStateChanged_ShouldRemoveWatcher_WhenDeactivated()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\unwatched",
            SafePath = "c:/repos/unwatched",
            Name = "unwatched",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\unwatched", ".git"))
            .Returns(@"c:\repos\unwatched\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\unwatched\.git"))
            .Returns(true);
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._))
            .Returns(Observable.Empty<RepositoryChangeEvent>());

        await _sut.StartAsync();
        _sut.ActivateMonitoring("c:/repos/unwatched");

        // Act
        Action act = () => _sut.DeactivateMonitoring("c:/repos/unwatched");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task EnsureRepoWatcher_ShouldNotAddDuplicate_WhenCalledTwice()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\dup",
            SafePath = "c:/repos/dup",
            Name = "dup",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\dup", ".git"))
            .Returns(@"c:\repos\dup\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\dup\.git"))
            .Returns(true);
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._))
            .Returns(Observable.Empty<RepositoryChangeEvent>());

        await _sut.StartAsync();

        // Act — activate twice
        _sut.ActivateMonitoring("c:/repos/dup");
        _sut.DeactivateMonitoring("c:/repos/dup");
        _sut.ActivateMonitoring("c:/repos/dup");

        // Assert — should not throw
        _store.Count.Should().Be(1);
    }

    [Fact]
    public async Task EnsureRepoWatcher_ShouldSkip_WhenGitDirNotFound()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\nogit",
            SafePath = "c:/repos/nogit",
            Name = "nogit",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\nogit", ".git"))
            .Returns(@"c:\repos\nogit\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\nogit\.git"))
            .Returns(false);
        A.CallTo(() => _fileSystem.File.Exists(@"c:\repos\nogit\.git"))
            .Returns(false);

        await _sut.StartAsync();

        // Act
        Action act = () => _sut.ActivateMonitoring("c:/repos/nogit");

        // Assert
        act.Should().NotThrow();
        // Watcher.Watch should only have been called for root paths, not for this repo
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>.That.Contains(@"c:\repos\nogit\.git")))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task TryResolveGitDir_ShouldResolveWorktreeGitFile()
    {
        // Arrange — .git is a file with gitdir pointing elsewhere
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\worktree",
            SafePath = "c:/repos/worktree",
            Name = "worktree",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\worktree", ".git"))
            .Returns(@"c:\repos\worktree\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\worktree\.git"))
            .Returns(false);
        A.CallTo(() => _fileSystem.File.Exists(@"c:\repos\worktree\.git"))
            .Returns(true);
        A.CallTo(() => _fileSystem.File.ReadAllLines(@"c:\repos\worktree\.git"))
            .Returns(["gitdir: c:\\repos\\main\\.git\\worktrees\\worktree"]);
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\main\.git\worktrees\worktree"))
            .Returns(true);
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._))
            .Returns(Observable.Empty<RepositoryChangeEvent>());

        await _sut.StartAsync();

        // Act
        _sut.ActivateMonitoring("c:/repos/worktree");

        // Assert — watcher should be set up for the resolved gitdir
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>.That.Contains(@"c:\repos\main\.git\worktrees\worktree")))
            .MustHaveHappened();
    }

    [Fact]
    public async Task TryResolveGitDir_ShouldReturnFalse_WhenWorktreeGitFileHasNoGitdirLine()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\badworktree",
            SafePath = "c:/repos/badworktree",
            Name = "badworktree",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\badworktree", ".git"))
            .Returns(@"c:\repos\badworktree\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\badworktree\.git"))
            .Returns(false);
        A.CallTo(() => _fileSystem.File.Exists(@"c:\repos\badworktree\.git"))
            .Returns(true);
        A.CallTo(() => _fileSystem.File.ReadAllLines(@"c:\repos\badworktree\.git"))
            .Returns(["some random content"]);

        await _sut.StartAsync();

        // Act
        Action act = () => _sut.ActivateMonitoring("c:/repos/badworktree");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task TryResolveGitDir_ShouldReturnFalse_WhenResolvedPathDoesNotExist()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\deadworktree",
            SafePath = "c:/repos/deadworktree",
            Name = "deadworktree",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\deadworktree", ".git"))
            .Returns(@"c:\repos\deadworktree\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\deadworktree\.git"))
            .Returns(false);
        A.CallTo(() => _fileSystem.File.Exists(@"c:\repos\deadworktree\.git"))
            .Returns(true);
        A.CallTo(() => _fileSystem.File.ReadAllLines(@"c:\repos\deadworktree\.git"))
            .Returns(["gitdir: c:\\nonexistent\\path"]);
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\nonexistent\path"))
            .Returns(false);

        await _sut.StartAsync();

        // Act
        Action act = () => _sut.ActivateMonitoring("c:/repos/deadworktree");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task TryResolveGitDir_ShouldHandleException_WhenReadingGitFileFails()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\errworktree",
            SafePath = "c:/repos/errworktree",
            Name = "errworktree",
        };
        _store.AddOrUpdate(repoInfo);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\errworktree", ".git"))
            .Returns(@"c:\repos\errworktree\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\errworktree\.git"))
            .Returns(false);
        A.CallTo(() => _fileSystem.File.Exists(@"c:\repos\errworktree\.git"))
            .Returns(true);
        A.CallTo(() => _fileSystem.File.ReadAllLines(@"c:\repos\errworktree\.git"))
            .Throws(new UnauthorizedAccessException("access denied"));

        await _sut.StartAsync();

        // Act
        Action act = () => _sut.ActivateMonitoring("c:/repos/errworktree");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task RefreshRepositoryAsync_ShouldReadAndReturnUpdatedRepo()
    {
        // Arrange
        var updatedRepo = new RepositoryInfo
        {
            Path = @"c:\repos\refreshed",
            SafePath = "c:/repos/refreshed",
            Name = "refreshed",
            CurrentBranch = "main",
        };
        A.CallTo(() => _reader.ReadAsync(@"c:\repos\refreshed", A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(updatedRepo));

        // Act
        var result = await _sut.RefreshRepositoryAsync(@"c:\repos\refreshed", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("refreshed");
        result.LastSeen.Should().BeOnOrAfter(DateTimeOffset.UtcNow.AddSeconds(-5));
        _store.Count.Should().Be(1);
    }

    [Fact]
    public async Task RefreshRepositoryAsync_ShouldReturnNull_WhenReaderReturnsNull()
    {
        // Arrange
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(null));

        // Act
        var result = await _sut.RefreshRepositoryAsync(@"c:\repos\missing", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshRepositoryAsync_ShouldReturnNull_WhenReaderThrows()
    {
        // Arrange
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new InvalidOperationException("read failure"));

        // Act
        var result = await _sut.RefreshRepositoryAsync(@"c:\repos\failing", CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RefreshRepositoryAsync_ShouldThrow_WhenPathIsNull()
    {
        // Act
        Func<Task> act = () => _sut.RefreshRepositoryAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RefreshRepositoryAsync_ShouldReturnCachedRepo_WhenRecentlyUpdated()
    {
        // Arrange — put a recently-updated repo in the store
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\cached",
            SafePath = "c:/repos/cached",
            Name = "cached",
            LastUpdated = DateTimeOffset.UtcNow,
        };
        _store.AddOrUpdate(repoInfo);

        // Act
        var result = await _sut.RefreshRepositoryAsync(@"c:\repos\cached", CancellationToken.None);

        // Assert — should return cached version without calling reader
        result.Should().BeSameAs(repoInfo);
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task RefreshAllAsync_ShouldSkipSecondCall_WhenAlreadyRunning()
    {
        // Arrange
        var fakeStore = A.Fake<IRepositoryStore>();
        using var gate = new SemaphoreSlim(0, 1);
        using var entered = new SemaphoreSlim(0, 1);

        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\blocking",
            SafePath = "c:/repos/blocking",
            Name = "blocking",
        };

        A.CallTo(() => fakeStore.Items).Returns([repoInfo]);
        _monitoringState.SetMonitored("c:/repos/blocking", true);

        A.CallTo(() => _fileSystem.Path.Combine(A<string>._, A<string>._, A<string>._))
            .Returns(@"c:\repos\blocking\.git\HEAD");
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .ReturnsLazily(async call =>
            {
                entered.Release();
                await gate.WaitAsync();
                return (RepositoryInfo?)repoInfo;
            });

        using var sut = new RepositoryMonitorService(
            _scanner, _watcher, _reader, fakeStore, _fileSystem, () => ["/repos"], _monitoringState, _monitoringState, _snapshotStore, NullLogger.Instance);

        // Act
        var firstCall = Task.Run(() => sut.RefreshAllAsync(CancellationToken.None));
        var didEnter = await entered.WaitAsync(TimeSpan.FromSeconds(5));
        didEnter.Should().BeTrue();

        // Second call should return immediately
        await sut.RefreshAllAsync(CancellationToken.None);

        gate.Release();
        await firstCall;

        // Assert — reader should only have been called once (from first call)
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task RefreshAllAsync_ShouldOnlyRefreshMonitoredRepos()
    {
        // Arrange
        var monitoredRepo = new RepositoryInfo
        {
            Path = @"c:\repos\monitored",
            SafePath = "c:/repos/monitored",
            Name = "monitored",
        };
        var unmonitoredRepo = new RepositoryInfo
        {
            Path = @"c:\repos\unmonitored",
            SafePath = "c:/repos/unmonitored",
            Name = "unmonitored",
        };

        _store.AddOrUpdate(monitoredRepo);
        _store.AddOrUpdate(unmonitoredRepo);
        _monitoringState.SetMonitored("c:/repos/monitored", true);
        // unmonitored is not set as monitored

        var updatedRepo = new RepositoryInfo
        {
            Path = @"c:\repos\monitored",
            SafePath = "c:/repos/monitored",
            Name = "monitored",
        };

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\monitored", ".git", "HEAD"))
            .Returns(@"c:\repos\monitored\.git\HEAD");
        A.CallTo(() => _reader.ReadAsync(@"c:\repos\monitored\.git\HEAD", A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(updatedRepo));

        // Act
        await _sut.RefreshAllAsync(CancellationToken.None);

        // Assert — reader should only be called for the monitored repo
        A.CallTo(() => _reader.ReadAsync(@"c:\repos\monitored\.git\HEAD", A<CancellationToken>._)).MustHaveHappened();
        A.CallTo(() => _reader.ReadAsync(A<string>.That.Contains("unmonitored"), A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ReadAndUpdateRepositoryAsync_ShouldRetryOnFailure()
    {
        // Arrange
        var watchSubject = new Subject<RepositoryChangeEvent>();
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(watchSubject.AsObservable());

        var callCount = 0;
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\retry",
            SafePath = "c:/repos/retry",
            Name = "retry",
        };

        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .ReturnsLazily(() =>
            {
                callCount++;
                if (callCount < 3)
                {
                    throw new InvalidOperationException("transient failure");
                }

                return Task.FromResult<RepositoryInfo?>(repoInfo);
            });

        await _sut.StartAsync();

        // Act — trigger change detection which calls ReadAndUpdateRepositoryAsync
        watchSubject.OnNext(new RepositoryChangeEvent(@"c:\repos\retry\.git\HEAD", RepositoryChangeType.Added));

        // Assert — give async pipeline time to complete retries
        await Task.Delay(1000);
        callCount.Should().Be(3);
        _store.Count.Should().Be(1);
    }

    [Fact]
    public async Task ReadAndUpdateRepositoryAsync_ShouldGiveUpAfterMaxRetries()
    {
        // Arrange
        var watchSubject = new Subject<RepositoryChangeEvent>();
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(watchSubject.AsObservable());

        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .ThrowsAsync(new InvalidOperationException("persistent failure"));

        await _sut.StartAsync();

        // Act
        watchSubject.OnNext(new RepositoryChangeEvent(@"c:\repos\failing\.git\HEAD", RepositoryChangeType.Modified));

        // Assert
        await Task.Delay(1000);
        _store.Count.Should().Be(0);
        A.CallTo(() => _reader.ReadAsync(@"c:\repos\failing\.git\HEAD", A<CancellationToken>._))
            .MustHaveHappened(3, Times.Exactly);
    }

    [Fact]
    public async Task ReadAndUpdateRepositoryAsync_ShouldSetLastUpdated_WhenRepoRead()
    {
        // Arrange
        var watchSubject = new Subject<RepositoryChangeEvent>();
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(watchSubject.AsObservable());

        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\updated",
            SafePath = "c:/repos/updated",
            Name = "updated",
        };
        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(repoInfo));

        var beforeUpdate = DateTimeOffset.UtcNow;
        await _sut.StartAsync();

        // Act
        watchSubject.OnNext(new RepositoryChangeEvent(@"c:\repos\updated\.git\HEAD", RepositoryChangeType.Added));

        await Task.Delay(500);

        // Assert
        repoInfo.LastSeen.Should().BeOnOrAfter(beforeUpdate);
        repoInfo.LastUpdated.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public async Task ReadAndUpdateRepositoryAsync_ShouldNotAddToStore_WhenReaderReturnsNull()
    {
        // Arrange
        var watchSubject = new Subject<RepositoryChangeEvent>();
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(watchSubject.AsObservable());

        A.CallTo(() => _reader.ReadAsync(A<string>._, A<CancellationToken>._))
            .Returns(Task.FromResult<RepositoryInfo?>(null));

        await _sut.StartAsync();

        // Act
        watchSubject.OnNext(new RepositoryChangeEvent(@"c:\repos\nullrepo\.git\HEAD", RepositoryChangeType.Added));

        await Task.Delay(500);

        // Assert
        _store.Count.Should().Be(0);
    }

    [Fact]
    public async Task StartAsync_ShouldSetupRepoWatchers_ForMonitoredReposInStore()
    {
        // Arrange — add a monitored repo to the store before starting
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\premonitored",
            SafePath = "c:/repos/premonitored",
            Name = "premonitored",
        };
        _store.AddOrUpdate(repoInfo);
        _monitoringState.SetMonitored("c:/repos/premonitored", true);

        A.CallTo(() => _fileSystem.Path.Combine(@"c:\repos\premonitored", ".git"))
            .Returns(@"c:\repos\premonitored\.git");
        A.CallTo(() => _fileSystem.Directory.Exists(@"c:\repos\premonitored\.git"))
            .Returns(true);
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._))
            .Returns(Observable.Empty<RepositoryChangeEvent>());

        // Act
        await _sut.StartAsync();

        // Allow time for the store subscription to process
        await Task.Delay(200);

        // Assert — watcher should have been set up for the repo's .git dir
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>.That.Contains(@"c:\repos\premonitored\.git")))
            .MustHaveHappened();
    }

    [Fact]
    public async Task OnRepositoryChangeDetected_ShouldHandleRemoval_OfExistingRepo_WithGitSuffix()
    {
        // Arrange
        var repoInfo = new RepositoryInfo
        {
            Path = @"c:\repos\removeme",
            SafePath = "c:/repos/removeme",
            Name = "removeme",
        };
        _store.AddOrUpdate(repoInfo);

        var watchSubject = new Subject<RepositoryChangeEvent>();
        A.CallTo(() => _watcher.Watch(A<IEnumerable<string>>._)).Returns(watchSubject.AsObservable());

        await _sut.StartAsync();

        // Act — send a removal event with backslash path and .git suffix
        watchSubject.OnNext(new RepositoryChangeEvent(@"c:\repos\removeme\.git\HEAD", RepositoryChangeType.Removed));

        await Task.Delay(100);

        // Assert
        _store.Count.Should().Be(0);
    }

    [Fact]
    public async Task CancelAllScans_ShouldCancelActiveScan()
    {
        // Arrange
        var scanStarted = new SemaphoreSlim(0, 1);
        var scanSubject = new Subject<string>();

        A.CallTo(() => _scanner.Scan(A<IEnumerable<string>>._, A<CancellationToken>._))
            .ReturnsLazily(call =>
            {
                scanStarted.Release();
                return scanSubject.AsObservable();
            });

        // Act
        var scanTask = _sut.ScanAsync(CancellationToken.None);
        await scanStarted.WaitAsync(TimeSpan.FromSeconds(5));
        _sut.CancelAllScans();

        // Assert
        Func<Task> act = () => scanTask;
        await act.Should().ThrowAsync<TaskCanceledException>();
    }

    public void Dispose()
    {
        _sut.Dispose();
        _store.Dispose();
    }
}
