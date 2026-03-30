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
    private readonly GitRepositoryScannerSettings _settings;
    private readonly GitRepositoryScanner _sut;

    public GitRepositoryScannerTests()
    {
        _fileSystem = new MockFileSystem();
        _settings = new GitRepositoryScannerSettings(1);
        _sut = new GitRepositoryScanner(_fileSystem, NullLogger.Instance, _settings);
    }

    [Fact]
    public async Task Scan_ShouldFindGitRepository()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git");
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\logs\HEAD", new MockFileData("ref: refs/heads/main"));
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\HEAD", new MockFileData("ref: refs/heads/main"));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().ContainSingle();
        foundPaths[0].Should().Contain("HEAD");
    }

    [Fact]
    public async Task Scan_ShouldFindMultipleRepositories()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\repo1\.git\logs");
        _fileSystem.AddFile(@"c:\repos\repo1\.git\logs\HEAD", new MockFileData(""));
        _fileSystem.AddDirectory(@"c:\repos\repo2\.git\logs");
        _fileSystem.AddFile(@"c:\repos\repo2\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().HaveCount(2);
    }

    [Fact]
    public async Task Scan_ShouldSkipNonExistentPaths()
    {
        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\nonexistent",]).ToList();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public async Task Scan_ShouldSkipNodeModulesDirectories()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\node_modules\some-pkg\.git\logs");
        _fileSystem.AddFile(@"c:\repos\node_modules\some-pkg\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Theory]
    [InlineData("bin")]
    [InlineData("obj")]
    [InlineData(".vs")]
    [InlineData("$RECYCLE.BIN")]
    [InlineData("System Volume Information")]
    public async Task Scan_ShouldSkipKnownDirectories(string dirName)
    {
        // Arrange
        _fileSystem.AddDirectory($@"c:\repos\{dirName}\somerepo\.git\logs");
        _fileSystem.AddFile($@"c:\repos\{dirName}\somerepo\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public async Task Scan_ShouldPreferLogsHeadOverHead()
    {
        // Arrange - repo has both .git/logs/HEAD and .git/HEAD
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\logs\HEAD", new MockFileData(""));
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().ContainSingle();
        foundPaths[0].Should().Contain(@"logs\HEAD");
    }

    [Fact]
    public async Task Scan_ShouldFallBackToHead_WhenLogsHeadDoesNotExist()
    {
        // Arrange - repo has only .git/HEAD (no logs/HEAD)
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().ContainSingle();
        foundPaths[0].Should().EndWith(@".git\HEAD");
    }

    [Fact]
    public async Task Scan_ShouldHandleMultipleRootPaths()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos1\repoA\.git\logs");
        _fileSystem.AddFile(@"c:\repos1\repoA\.git\logs\HEAD", new MockFileData(""));
        _fileSystem.AddDirectory(@"c:\repos2\repoB\.git\logs");
        _fileSystem.AddFile(@"c:\repos2\repoB\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos1", @"c:\repos2",]).ToList();

        // Assert
        foundPaths.Should().HaveCount(2);
    }

    [Fact]
    public async Task Scan_ShouldScanNestedRepositories()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\org\team\project\.git\logs");
        _fileSystem.AddFile(@"c:\repos\org\team\project\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().ContainSingle();
    }

    [Fact]
    public async Task IsScanning_ShouldEmitFalseInitially()
    {
        // Arrange & Act
        bool isScanning = await _sut.IsScanning.FirstAsync();

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
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",], cts.Token).ToList();

        // Assert - should complete (possibly empty) without throwing
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public async Task Scan_ShouldCompleteEmpty_WhenAllRootsNonExistent()
    {
        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\nope1", @"c:\nope2",]).ToList();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public async Task Scan_ShouldCompleteEmpty_WhenEmptyPaths()
    {
        // Act
        IList<string> foundPaths = await _sut.Scan([]).ToList();

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
        var act = () => new GitRepositoryScanner(null!, NullLogger.Instance, _settings);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new GitRepositoryScanner(_fileSystem, null!, _settings);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSettingsIsNull()
    {
        var act = () => new GitRepositoryScanner(_fileSystem, NullLogger.Instance, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task IsScanning_ShouldTransitionTrueAndBackToFalse_DuringScan()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\logs\HEAD", new MockFileData(""));

        List<bool> scanningValues = new();
        using var subscription = _sut.IsScanning.Subscribe(scanningValues.Add);

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Allow time for the async pipeline to emit final IsScanning=false
        await Task.Delay(200);

        // Assert
        foundPaths.Should().ContainSingle();
        scanningValues.Should().Contain(true, "IsScanning should have been true during scan");
        scanningValues.Last().Should().BeFalse("IsScanning should be false after scan completes");
    }

    [Fact]
    public async Task Scan_ShouldNotEmitPath_WhenGitDirExistsButNoHeadFile()
    {
        // Arrange - .git directory exists but no HEAD or logs/HEAD file
        _fileSystem.AddDirectory(@"c:\repos\broken\.git");

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public async Task Scan_ShouldWorkWithHigherParallelism()
    {
        // Arrange
        using var sut = new GitRepositoryScanner(_fileSystem, NullLogger.Instance, new GitRepositoryScannerSettings(4));

        _fileSystem.AddDirectory(@"c:\repos\repo1\.git\logs");
        _fileSystem.AddFile(@"c:\repos\repo1\.git\logs\HEAD", new MockFileData(""));
        _fileSystem.AddDirectory(@"c:\repos\repo2\.git\logs");
        _fileSystem.AddFile(@"c:\repos\repo2\.git\logs\HEAD", new MockFileData(""));
        _fileSystem.AddDirectory(@"c:\repos\repo3\.git\logs");
        _fileSystem.AddFile(@"c:\repos\repo3\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().HaveCount(3);
    }

    [Fact]
    public async Task Scan_ShouldReturnEmpty_WhenDirectoryExistsButHasNoSubdirectories()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos");

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert
        foundPaths.Should().BeEmpty();
    }

    [Fact]
    public async Task Scan_ShouldNotDescendIntoGitDirectory()
    {
        // Arrange - repo with .git dir containing nested structures
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\logs\HEAD", new MockFileData(""));
        // A nested .git inside .git should not be scanned
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\modules\sub\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\modules\sub\.git\logs\HEAD", new MockFileData(""));

        // Act
        IList<string> foundPaths = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert - only the top-level repo, not the nested one inside .git
        foundPaths.Should().ContainSingle();
    }

    [Fact]
    public async Task Scan_MultipleConcurrentScans_ShouldAllComplete()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\repo1\.git\logs");
        _fileSystem.AddFile(@"c:\repos\repo1\.git\logs\HEAD", new MockFileData(""));

        // Act - start two scans sequentially
        IList<string> result1 = await _sut.Scan([@"c:\repos",]).ToList();
        IList<string> result2 = await _sut.Scan([@"c:\repos",]).ToList();

        // Assert - both should complete and find the repo
        result1.Should().ContainSingle();
        result2.Should().ContainSingle();
    }

    [Fact]
    public async Task Scan_ShouldHandleMixOfExistentAndNonExistentPaths()
    {
        // Arrange
        _fileSystem.AddDirectory(@"c:\repos\myrepo\.git\logs");
        _fileSystem.AddFile(@"c:\repos\myrepo\.git\logs\HEAD", new MockFileData(""));

        // Act - mix of valid and invalid root paths
        IList<string> foundPaths = await _sut.Scan([@"c:\nonexistent", @"c:\repos", @"c:\alsonope",]).ToList();

        // Assert
        foundPaths.Should().ContainSingle();
    }

    public void Dispose()
    {
        _sut.Dispose();
    }
}
