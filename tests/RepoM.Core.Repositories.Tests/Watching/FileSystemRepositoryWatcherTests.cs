namespace RepoM.Core.Repositories.Tests.Watching;

using System;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Reactive.Linq;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Repositories.Watching;
using Xunit;

public class FileSystemRepositoryWatcherTests : IDisposable
{
    private readonly MockFileSystem _fileSystem;
    private readonly FileSystemRepositoryWatcher _sut;

    public FileSystemRepositoryWatcherTests()
    {
        _fileSystem = new MockFileSystem();
        _sut = new FileSystemRepositoryWatcher(_fileSystem, NullLogger.Instance);
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
        var act = () => new FileSystemRepositoryWatcher(_fileSystem, null!);
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
    public void Watch_ShouldNotEmitEvents_WhenPathDoesNotExist()
    {
        // Act - Watch is long-running (never completes), so use Take with timeout
        var hasEvents = false;
        using var subscription = _sut.Watch(["/nonexistent",])
            .Take(TimeSpan.FromMilliseconds(200))
            .Subscribe(_ => hasEvents = true);

        Thread.Sleep(300);

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

    public void Dispose()
    {
        _sut.Dispose();
    }
}
