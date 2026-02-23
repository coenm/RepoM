namespace RepoM.Core.Repositories.Tests.Watching;

using System;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Repositories.Watching;
using Xunit;

public class FileSystemRepositoryWatcherTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenFileSystemIsNull()
    {
        var act = () => new FileSystemRepositoryWatcher(null!, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var fileSystem = new MockFileSystem();
        var act = () => new FileSystemRepositoryWatcher(fileSystem, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Watch_ShouldThrow_WhenPathsIsNull()
    {
        var fileSystem = new MockFileSystem();
        var sut = new FileSystemRepositoryWatcher(fileSystem, NullLogger.Instance);
        var act = () => sut.Watch(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
