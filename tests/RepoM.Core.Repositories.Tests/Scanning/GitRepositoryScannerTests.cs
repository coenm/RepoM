namespace RepoM.Core.Repositories.Tests.Scanning;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        foundPaths.Should().ContainSingle();
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

    [Theory]
    [InlineData("bin")]
    [InlineData("obj")]
    [InlineData(".vs")]
    [InlineData("$RECYCLE.BIN")]
    [InlineData("System Volume Information")]
    public void Scan_ShouldSkipKnownDirectories(string dirName)
    {
        // Arrange
        _fileSystem.AddDirectory($@"c:\repos\{dirName}\somerepo\.git\logs");
        _fileSystem.AddFile($@"c:\repos\{dirName}\somerepo\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\repos",]).ToList().Wait();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public void Scan_ShouldPreferLogsHeadOverHead()
    {
        // Arrange - repo has both .git/logs/HEAD and .git/HEAD
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\logs\HEAD", new MockFileData(""));
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\repos",]).ToList().Wait();

        // Assert
        foundPaths.Should().ContainSingle();
        foundPaths[0].Should().Contain(@"logs\HEAD");
    }

    [Fact]
    public void Scan_ShouldFallBackToHead_WhenLogsHeadDoesNotExist()
    {
        // Arrange - repo has only .git/HEAD (no logs/HEAD)
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\repos",]).ToList().Wait();

        // Assert
        foundPaths.Should().ContainSingle();
        foundPaths[0].Should().EndWith(@".git\HEAD");
    }

    [Fact]
    public void Scan_ShouldHandleMultipleRootPaths()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos1\repoA\.git\logs");
        _fileSystem.AddFile(@"c:\repos1\repoA\.git\logs\HEAD", new MockFileData(""));
        _fileSystem.AddDirectory(@"c:\repos2\repoB\.git\logs");
        _fileSystem.AddFile(@"c:\repos2\repoB\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\repos1", @"c:\repos2",]).ToList().Wait();

        // Assert
        foundPaths.Should().HaveCount(2);
    }

    [Fact]
    public void Scan_ShouldScanNestedRepositories()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\org\team\project\.git\logs");
        _fileSystem.AddFile(@"c:\repos\org\team\project\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\repos",]).ToList().Wait();

        // Assert
        foundPaths.Should().ContainSingle();
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

    [Fact]
    public async Task Scan_ShouldSupportCancellation()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\logs\HEAD", new MockFileData(""));

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // pre-cancel

        // Act
        var foundPaths = await _sut.Scan([@"c:\repos",], cts.Token).ToList();

        // Assert - should complete (possibly empty) without throwing
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public void Scan_ShouldCompleteEmpty_WhenAllRootsNonExistent()
    {
        // Act
        IList<string> foundPaths = _sut.Scan([@"c:\nope1", @"c:\nope2",]).ToList().Wait();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public void Scan_ShouldCompleteEmpty_WhenEmptyPaths()
    {
        // Act
        IList<string> foundPaths = _sut.Scan([]).ToList().Wait();

        // Assert
        foundPaths.Should().BeEmpty();
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
    public void Constructor_ShouldThrow_WhenFileSystemIsNull()
    {
        var act = () => new GitRepositoryScanner(null!, NullLogger.Instance);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new GitRepositoryScanner(_fileSystem, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
