namespace RepoM.Core.Repositories.Tests.Scanning;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive.Linq;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Repositories.Scanning;
using Xunit;

public class GitRepositoryScannerTests : IDisposable
{
    private readonly MockFileSystem _fileSystem;
    private readonly GitRepositoryScanner _sut;

    public GitRepositoryScannerTests()
    {
        _fileSystem = new MockFileSystem();
        _sut = new GitRepositoryScanner(_fileSystem, NullLogger.Instance);
    }

    [Fact]
    public void Scan_ShouldFindGitRepository()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git");
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\logs\HEAD", new MockFileData("ref: refs/heads/main"));
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\HEAD", new MockFileData("ref: refs/heads/main"));

        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\repos",]).ToList().Wait();

        // Assert
        foundPaths.Should().HaveCount(1);
        foundPaths[0].Should().Contain("HEAD");
    }

    [Fact]
    public void Scan_ShouldFindMultipleRepositories()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\repo1\.git\logs");
        _fileSystem.AddFile(@"c:\repos\repo1\.git\logs\HEAD", new MockFileData(""));
        _fileSystem.AddDirectory(@"c:\repos\repo2\.git\logs");
        _fileSystem.AddFile(@"c:\repos\repo2\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\repos",]).ToList().Wait();

        // Assert
        foundPaths.Should().HaveCount(2);
    }

    [Fact]
    public void Scan_ShouldSkipNonExistentPaths()
    {
        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\nonexistent",]).ToList().Wait();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public void Scan_ShouldSkipNodeModulesDirectories()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\node_modules\some-pkg\.git\logs");
        _fileSystem.AddFile(@"c:\repos\node_modules\some-pkg\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\repos",]).ToList().Wait();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public void IsScanning_ShouldEmitFalseInitially()
    {
        // Arrange & Act
        var isScanning = _sut.IsScanning.FirstAsync().Wait();

        // Assert
        isScanning.Should().BeFalse();
    }

    [Fact]
    public void Scan_ShouldThrow_WhenPathsIsNull()
    {
        var act = () => _sut.Scan(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
