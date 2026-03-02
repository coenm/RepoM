namespace RepoM.Core.Repositories.Tests.Watching;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Watching;
using Xunit;

public class FileSystemRepositoryWatcherTests : IDisposable
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly FileSystemRepositoryWatcher _sut;

    public FileSystemRepositoryWatcherTests()
    {
        _mockFileSystem = new MockFileSystem();
        _sut = new FileSystemRepositoryWatcher(_mockFileSystem, NullLogger.Instance);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenFileSystemIsNull()
    {
        var act = () => new FileSystemRepositoryWatcher(null!, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new FileSystemRepositoryWatcher(_mockFileSystem, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Watch_ShouldThrow_WhenPathsIsNull()
    {
        var act = () => _sut.Watch(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Watch_ShouldReturnObservable_WhenPathsAreEmpty()
    {
        // Act
        var observable = _sut.Watch([]);

        // Assert
        observable.Should().NotBeNull();
    }

    [Fact]
    public async Task Watch_ShouldNotEmitEvents_WhenPathDoesNotExist()
    {
        // Act - Watch is long-running (never completes), so use Take with timeout
        var hasEvents = false;
        using var subscription = _sut.Watch(["/nonexistent",])
            .Take(TimeSpan.FromMilliseconds(200))
            .Subscribe(_ => hasEvents = true);

        await Task.Delay(300);

        // Assert
        hasEvents.Should().BeFalse();
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
    public async Task Watch_ShouldEmitAddedEvent_WhenHeadFileIsCreated()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Act
        dummyWatcher.SimulateCreated(@"c:\repos\myrepo\.git\logs", "HEAD");

        // Assert
        mre.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        events.Should().ContainSingle();
        events[0].ChangeType.Should().Be(RepositoryChangeType.Added);
        events[0].Path.Should().Be(@"c:\repos\myrepo\.git\logs\HEAD");
    }

    [Fact]
    public async Task Watch_ShouldEmitModifiedEvent_WhenHeadFileIsChanged()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Act
        dummyWatcher.SimulateChanged(@"c:\repos\myrepo\.git\logs", "HEAD");

        // Assert
        mre.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        events.Should().ContainSingle();
        events[0].ChangeType.Should().Be(RepositoryChangeType.Modified);
    }

    [Fact]
    public async Task Watch_ShouldEmitRemovedEvent_WhenHeadFileIsDeleted()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Act
        dummyWatcher.SimulateDeleted(@"c:\repos\myrepo\.git\logs", "HEAD");

        // Assert
        mre.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        events.Should().ContainSingle();
        events[0].ChangeType.Should().Be(RepositoryChangeType.Removed);
    }

    [Fact]
    public async Task Watch_ShouldEmitModifiedEvent_WhenHeadFileIsRenamed()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Act
        dummyWatcher.SimulateRenamed(@"c:\repos\myrepo\.git\logs", "HEAD", "HEAD.old");

        // Assert
        mre.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        events.Should().ContainSingle();
        events[0].ChangeType.Should().Be(RepositoryChangeType.Modified);
    }

    [Fact]
    public async Task Watch_ShouldIgnoreNonGitPaths()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Act - file not in a .git directory
        dummyWatcher.SimulateChanged(@"c:\repos\myrepo\src", "HEAD");

        // Assert - no event expected, wait briefly to confirm
        mre.Wait(TimeSpan.FromSeconds(1)).Should().BeFalse();
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task Watch_ShouldIgnoreNonHeadFiles()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Act - file in .git but not HEAD
        dummyWatcher.SimulateChanged(@"c:\repos\myrepo\.git", "config");

        // Assert - no event expected, wait briefly to confirm
        mre.Wait(TimeSpan.FromSeconds(1)).Should().BeFalse();
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task Watch_ShouldDebounceRapidChangesForSameRepository()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Act - rapid changes to the same repo
        dummyWatcher.SimulateChanged(@"c:\repos\myrepo\.git\logs", "HEAD");
        dummyWatcher.SimulateChanged(@"c:\repos\myrepo\.git\logs", "HEAD");
        dummyWatcher.SimulateChanged(@"c:\repos\myrepo\.git\logs", "HEAD");

        // Assert - wait for first (and only) debounced event
        mre.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();

        // Wait a bit more to confirm no additional events arrive
        await Task.Delay(700);
        events.Should().ContainSingle();
    }

    [Fact]
    public async Task Watch_ShouldEmitSeparateEventsForDifferentRepositories()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new CountdownEvent(2);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Signal();
        });

        // Act - changes to different repos
        dummyWatcher.SimulateChanged(@"c:\repos\repo1\.git\logs", "HEAD");
        dummyWatcher.SimulateChanged(@"c:\repos\repo2\.git\logs", "HEAD");

        // Assert - two events, one per repo
        mre.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        events.Should().HaveCount(2);
    }

    [Fact]
    public void Watch_ShouldConfigureWatcherCorrectly()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var fileSystemWatcher = A.Fake<IFileSystemWatcher>(o => o.Wrapping(dummyWatcher));
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.FileSystemWatcher.New(@"c:\repos")).Returns(fileSystemWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);
        var sut = new FileSystemRepositoryWatcher(fileSystem, NullLogger.Instance);

        // Act
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(_ => { });

        // Assert
        A.CallToSet(() => fileSystemWatcher.Filter).To("HEAD").MustHaveHappened();
        A.CallToSet(() => fileSystemWatcher.IncludeSubdirectories).To(true).MustHaveHappened();
        A.CallToSet(() => fileSystemWatcher.EnableRaisingEvents).To(true).MustHaveHappened();
    }

    [Fact]
    public async Task Watch_ShouldHandleWatcherCreationFailure()
    {
        // Arrange
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);
        A.CallTo(() => fileSystem.FileSystemWatcher.New(@"c:\repos")).Throws(new IOException("access denied"));
        var sut = new FileSystemRepositoryWatcher(fileSystem, NullLogger.Instance);

        // Act - should not throw, just log the error
        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        using var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Assert - no event expected
        mre.Wait(TimeSpan.FromSeconds(1)).Should().BeFalse();
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task Watch_ShouldStopRaisingEvents_WhenSubscriptionDisposed()
    {
        // Arrange
        var dummyWatcher = new DummyFileSystemWatcher();
        var (fileSystem, sut) = CreateSutWithFakedFileSystem(dummyWatcher);
        A.CallTo(() => fileSystem.Directory.Exists(@"c:\repos")).Returns(true);

        using var mre = new ManualResetEventSlim(false);
        var events = new List<RepositoryChangeEvent>();
        var subscription = sut.Watch([@"c:\repos",]).Subscribe(e =>
        {
            events.Add(e);
            mre.Set();
        });

        // Act
        subscription.Dispose();
        dummyWatcher.SimulateChanged(@"c:\repos\myrepo\.git\logs", "HEAD");

        // Assert - no event expected
        mre.Wait(TimeSpan.FromSeconds(1)).Should().BeFalse();
        events.Should().BeEmpty();
    }

    private static (IFileSystem fileSystem, FileSystemRepositoryWatcher sut) CreateSutWithFakedFileSystem(DummyFileSystemWatcher dummyWatcher)
    {
        var fileSystemWatcher = A.Fake<IFileSystemWatcher>(o => o.Wrapping(dummyWatcher));
        var fileSystem = A.Fake<IFileSystem>();
        A.CallTo(() => fileSystem.FileSystemWatcher.New(A<string>._)).Returns(fileSystemWatcher);
        var sut = new FileSystemRepositoryWatcher(fileSystem, NullLogger.Instance);
        return (fileSystem, sut);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
