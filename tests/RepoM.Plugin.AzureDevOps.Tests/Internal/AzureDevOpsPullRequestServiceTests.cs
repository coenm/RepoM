namespace RepoM.Plugin.AzureDevOps.Tests.Internal;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.AzureDevOps.Internal;
using Xunit;

public class AzureDevOpsPullRequestServiceTests
{
    private readonly IAzureDevopsConfiguration _configuration = A.Fake<IAzureDevopsConfiguration>();
    private readonly ILogger _logger = A.Fake<ILogger>();

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<AzureDevOpsPullRequestService> act1 = () => new AzureDevOpsPullRequestService(A.Dummy<IAzureDevopsConfiguration>(), null!);
        Func<AzureDevOpsPullRequestService> act2 = () => new AzureDevOpsPullRequestService(null!, A.Dummy<ILogger>());

        // assert
        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_ShouldNotThrow_WhenPatIsNullOrEmpty()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);

        // act
        Action act = () =>
        {
            using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);
        };

        // assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnCompletedTask_WhenConnectionIsNull()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        // act
        await sut.InitializeAsync();

        // assert (no exception thrown, method returns)
    }

    [Fact]
    public void GetPullRequests_ShouldReturnEmptyList_WhenNotInitialized()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>());

        // act
        List<PullRequest> result = sut.GetPullRequests(repository, "myProject", null);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPullRequests_ShouldReturnEmptyList_WhenRepoIdIsProvidedButNotInitialized()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>());

        // act
        List<PullRequest> result = sut.GetPullRequests(repository, "myProject", Guid.NewGuid().ToString());

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CountPullRequests_ShouldReturnZero_WhenNoRepositoriesKnown()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>());

        // act
        var result = sut.CountPullRequests(repository);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenNotInitialized()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        // act
        Action act = () => sut.Dispose();

        // assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        // act
        Action act = () =>
        {
            sut.Dispose();
            sut.Dispose();
        };

        // assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnCompletedTask_WhenPatIsEmpty()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(string.Empty);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        // act
        await sut.InitializeAsync();

        // assert (no exception thrown, returns without initializing timers)
    }

    [Fact]
    public void GetPullRequests_ShouldReturnEmptyList_WhenRepoIdIsInvalidGuid()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>());

        // act
        List<PullRequest> result = sut.GetPullRequests(repository, "myProject", "not-a-valid-guid");

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPullRequests_ShouldReturnEmptyList_WhenRepoIdIsEmptyGuid()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>());

        // act
        List<PullRequest> result = sut.GetPullRequests(repository, "myProject", Guid.Empty.ToString());

        // assert
        result.Should().BeEmpty();
    }
}
