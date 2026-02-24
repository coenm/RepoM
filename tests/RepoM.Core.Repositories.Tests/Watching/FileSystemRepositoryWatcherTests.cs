namespace RepoM.Core.Repositories.Tests.Watching;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
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
    public void Watch_ShouldCompleteEmpty_WhenPathDoesNotExist()
    {
        // Act
        var events = _sut.Watch(["/nonexistent",]).ToList().Wait();

        // Assert
        events.Should().BeEmpty();
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
