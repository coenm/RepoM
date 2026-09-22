namespace RepoM.Plugin.Mcp.Tests;

using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Store;
using RepoM.Plugin.Mcp.Tools;
using Xunit;

public class RepositoryToolsTests
{
    private readonly IRepositoryStore _repositoryStore;

    public RepositoryToolsTests()
    {
        _repositoryStore = A.Fake<IRepositoryStore>();
    }

    [Fact]
    public void ListRepositories_ShouldReturnAllRepositories_WhenNoFilterIsSpecified()
    {
        // arrange
        A.CallTo(() => _repositoryStore.Items).Returns(CreateSampleRepositories());

        // act
        var result = RepositoryTools.ListRepositories(_repositoryStore);

        // assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ListRepositories_ShouldFilterByName_WhenNameFilterIsSpecified()
    {
        // arrange
        A.CallTo(() => _repositoryStore.Items).Returns(CreateSampleRepositories());

        // act
        var result = RepositoryTools.ListRepositories(_repositoryStore, nameFilter: "RepoA");

        // assert
        Assert.Single(result);
        Assert.Equal("RepoA", result[0].Name);
    }

    [Fact]
    public void ListRepositories_ShouldReturnEmpty_WhenNoRepositoriesMatchFilter()
    {
        // arrange
        A.CallTo(() => _repositoryStore.Items).Returns(CreateSampleRepositories());

        // act
        var result = RepositoryTools.ListRepositories(_repositoryStore, nameFilter: "NonExistent");

        // assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindRepositories_ShouldFilterByBranch()
    {
        // arrange
        A.CallTo(() => _repositoryStore.Items).Returns(CreateSampleRepositories());

        // act
        var result = RepositoryTools.FindRepositories(_repositoryStore, branch: "main");

        // assert
        Assert.Single(result);
        Assert.Equal("RepoA", result[0].Name);
    }

    [Fact]
    public void FindRepositories_ShouldFilterByHasLocalChanges()
    {
        // arrange
        A.CallTo(() => _repositoryStore.Items).Returns(CreateSampleRepositories());

        // act
        var result = RepositoryTools.FindRepositories(_repositoryStore, hasLocalChanges: true);

        // assert
        Assert.Single(result);
        Assert.Equal("RepoB", result[0].Name);
    }

    [Fact]
    public void GetRepository_ShouldReturnNull_WhenRepositoryNotFound()
    {
        // arrange
        A.CallTo(() => _repositoryStore.Items).Returns(CreateSampleRepositories());
        A.CallTo(() => _repositoryStore.Lookup(A<string>._)).Returns(DynamicData.Kernel.Optional<RepositoryInfo>.None);

        // act
        var result = RepositoryTools.GetRepository(_repositoryStore, "c:/nonexistent");

        // assert
        Assert.Null(result);
    }

    [Fact]
    public void GetRepository_ShouldReturnRepository_WhenPathMatches()
    {
        // arrange
        var repos = CreateSampleRepositories().ToList();
        A.CallTo(() => _repositoryStore.Items).Returns(repos);
        A.CallTo(() => _repositoryStore.Lookup(A<string>._)).Returns(DynamicData.Kernel.Optional<RepositoryInfo>.None);

        // act
        var result = RepositoryTools.GetRepository(_repositoryStore, "C:/repos/RepoA");

        // assert
        Assert.NotNull(result);
        Assert.Equal("RepoA", result.Name);
    }

    private static IEnumerable<RepositoryInfo> CreateSampleRepositories()
    {
        yield return new RepositoryInfo
        {
            Name = "RepoA",
            Path = "C:/repos/RepoA",
            SafePath = "c:/repos/repoa",
            CurrentBranch = "main",
            Remotes = [new Remote("origin", "https://github.com/user/RepoA.git"),],
        };

        yield return new RepositoryInfo
        {
            Name = "RepoB",
            Path = "C:/repos/RepoB",
            SafePath = "c:/repos/repob",
            CurrentBranch = "feature/new-feature",
            LocalModified = 3,
            Remotes = [new Remote("origin", "https://github.com/user/RepoB.git"),],
        };
    }
}
