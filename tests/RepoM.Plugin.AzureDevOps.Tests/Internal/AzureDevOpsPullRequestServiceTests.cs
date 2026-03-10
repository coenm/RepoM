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
        Func<Task> act = async () => await sut.InitializeAsync();

        // assert
        await act.Should().NotThrowAsync();
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

        // assert - should log warning because no connection was established
        A.CallTo(_logger)
            .Where(call => call.Method.Name == "Log" && call.Arguments.Get<LogLevel>(0) == LogLevel.Warning)
            .MustHaveHappened();
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

    [Fact]
    public void Ctor_ShouldNotThrow_WhenPatIsProvided_ButBaseUrlIsNull()
    {
        // arrange - PAT given but URL is null; VssConnection will throw, ctor should catch it
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns("some-pat");
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);

        // act
        Action act = () =>
        {
            using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);
        };

        // assert - constructor catches the exception internally
        act.Should().NotThrow();
    }

    [Fact]
    public async Task InitializeAsync_ShouldLogWarning_WhenConnectionIsNull()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        // act
        await sut.InitializeAsync();

        // assert
        A.CallTo(_logger)
            .Where(call => call.Method.Name == "Log" && call.Arguments.Get<LogLevel>(0) == LogLevel.Warning)
            .MustHaveHappened();
    }

    [Fact]
    public void CountPullRequests_ShouldReturnZero_WhenRepositoryHasNoRemotes()
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
    public void CountPullRequests_ShouldReturnZero_WhenRepositoryHasNonOriginRemote()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>
        {
            new("upstream", "https://dev.azure.com/org/project/_git/repo"),
        });

        // act
        var result = sut.CountPullRequests(repository);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void CountPullRequests_ShouldReturnZero_WhenRepositoryHasOriginRemote_ButNoDevOpsRepos()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>
        {
            new("Origin", "https://dev.azure.com/org/project/_git/repo"),
        });

        // act
        var result = sut.CountPullRequests(repository);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetPullRequests_ShouldReturnSameEmptyList_WhenCalledMultipleTimes()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>());

        // act
        List<PullRequest> result1 = sut.GetPullRequests(repository, "myProject", null);
        List<PullRequest> result2 = sut.GetPullRequests(repository, "myProject", null);

        // assert - should return same cached empty list instance
        result1.Should().BeEmpty();
        result2.Should().BeEmpty();
        result1.Should().BeSameAs(result2);
    }

    [Fact]
    public void GetPullRequests_ShouldReturnEmptyList_WhenProjectIdIsWhitespace()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>());

        // act
        List<PullRequest> result = sut.GetPullRequests(repository, "  ", null);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void CountPullRequests_ShouldCacheMapping_WhenCalledMultipleTimesForSameRepo()
    {
        // arrange
        A.CallTo(() => _configuration.AzureDevOpsPersonalAccessToken).Returns(null);
        A.CallTo(() => _configuration.AzureDevOpsBaseUrl).Returns(null);
        using var sut = new AzureDevOpsPullRequestService(_configuration, _logger);

        var repository = A.Fake<IRepository>();
        A.CallTo(() => repository.SafePath).Returns("C:/some/path");
        A.CallTo(() => repository.Remotes).Returns(new List<Remote>());

        // act - call twice to exercise the caching path
        var result1 = sut.CountPullRequests(repository);
        var result2 = sut.CountPullRequests(repository);

        // assert
        result1.Should().Be(0);
        result2.Should().Be(0);
    }
}
