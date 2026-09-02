namespace RepoM.Core.Repositories.Tests.Persistence;

using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Persistence;
using Xunit;

public class RepositorySnapshotStoreTests
{
    private const string FILE_PATH = @"c:\appdata\repositories.snapshot.json";

    private readonly MockFileSystem _fileSystem = new();
    private readonly RepositorySnapshotStore _sut;

    public RepositorySnapshotStoreTests()
    {
        _sut = new RepositorySnapshotStore(
            _fileSystem,
            new RepositorySnapshotStoreSettings(FILE_PATH),
            NullLogger.Instance);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnEmpty_WhenFileDoesNotExist()
    {
        IReadOnlyList<RepositoryInfo> result = await _sut.LoadAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_ShouldRoundTripRepository()
    {
        // Arrange
        var repo = new RepositoryInfo
        {
            Path = @"c:\repos\myrepo",
            SafePath = "c:/repos/myrepo",
            Name = "myrepo",
            WindowsPath = @"c:\repos\myrepo",
            LinuxPath = "c:/repos/myrepo",
            Location = @"c:\repos",
            CurrentBranch = "main",
            CurrentBranchHasUpstream = true,
            Branches = ["main", "dev",],
            LocalBranches = ["main",],
            Tags = ["work", "personal",],
            AheadBy = 2,
            BehindBy = 1,
            LocalModified = 3,
            StashCount = 1,
        };
        repo.Remotes.Add(new Remote("origin", "https://example.com/repo.git"));

        // Act
        await _sut.SaveAsync([repo,]);
        IReadOnlyList<RepositoryInfo> loaded = await _sut.LoadAsync();

        // Assert
        loaded.Should().ContainSingle();
        RepositoryInfo actual = loaded[0];
        actual.SafePath.Should().Be("c:/repos/myrepo");
        actual.Name.Should().Be("myrepo");
        actual.CurrentBranch.Should().Be("main");
        actual.CurrentBranchHasUpstream.Should().BeTrue();
        actual.Branches.Should().BeEquivalentTo("main", "dev");
        actual.LocalBranches.Should().BeEquivalentTo("main");
        actual.Tags.Should().BeEquivalentTo("work", "personal");
        actual.AheadBy.Should().Be(2);
        actual.BehindBy.Should().Be(1);
        actual.LocalModified.Should().Be(3);
        actual.StashCount.Should().Be(1);
        actual.Remotes.Should().ContainSingle();
        actual.Remotes[0].Key.Should().Be("origin");
        actual.Remotes[0].Url.Should().Be("https://example.com/repo.git");
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var repo = new RepositoryInfo
        {
            Path = @"c:\repos\myrepo",
            SafePath = "c:/repos/myrepo",
            Name = "myrepo",
        };

        // Act
        await _sut.SaveAsync([repo,]);

        // Assert
        _fileSystem.File.Exists(FILE_PATH).Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnEmpty_WhenFileIsCorrupt()
    {
        // Arrange
        _fileSystem.AddFile(FILE_PATH, new MockFileData("this is not valid json"));

        // Act
        IReadOnlyList<RepositoryInfo> result = await _sut.LoadAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_ShouldRoundTripMultipleRepositories()
    {
        // Arrange
        var repo1 = new RepositoryInfo { Path = @"c:\r1", SafePath = "c:/r1", Name = "r1", };
        var repo2 = new RepositoryInfo { Path = @"c:\r2", SafePath = "c:/r2", Name = "r2", };

        // Act
        await _sut.SaveAsync([repo1, repo2,]);
        IReadOnlyList<RepositoryInfo> loaded = await _sut.LoadAsync();

        // Assert
        loaded.Should().HaveCount(2);
        loaded.Select(r => r.SafePath).Should().BeEquivalentTo("c:/r1", "c:/r2");
    }
}
