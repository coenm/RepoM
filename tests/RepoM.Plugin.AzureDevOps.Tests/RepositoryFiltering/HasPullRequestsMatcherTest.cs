namespace RepoM.Plugin.AzureDevOps.Tests.RepositoryFiltering;

using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Plugin.AzureDevOps.Internal;
using RepoM.Plugin.AzureDevOps.RepositoryFiltering;
using Xunit;

public class HasPullRequestsMatcherTest
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IAzureDevOpsPullRequestService _azureDevOpsPullRequestService = A.Fake<IAzureDevOpsPullRequestService>();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Ctor_ShouldThrow_WhenArgumentNull(bool ignoreCase)
    {
        // arrange

        // act
        Func<HasPullRequestsMatcher> act1 = () => new HasPullRequestsMatcher(null!, ignoreCase);

        // assert
        act1.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(InvalidTerms))]
    public void IsMatch_ShouldReturnNull(TermBase term)
    {
        // arrange
        var sut = new HasPullRequestsMatcher(_azureDevOpsPullRequestService, true);

        // act
        var result = sut.IsMatch(_repository, term);

        // assert
        result.Should().BeNull();
        A.CallTo(_azureDevOpsPullRequestService).MustNotHaveHappened();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    [Theory]
    [MemberData(nameof(ValidTerms))]
    public void IsMatch_ShouldReturnTrue_WhenServiceReturnsCountGreaterThenZero(TermBase term)
    {
        // arrange
        A.CallTo(() => _azureDevOpsPullRequestService.CountPullRequests(_repository)).Returns(42);
        var sut = new HasPullRequestsMatcher(_azureDevOpsPullRequestService, true);

        // act
        var result = sut.IsMatch(_repository, term);

        // assert
        result.Should().BeTrue();
        A.CallTo(_azureDevOpsPullRequestService).MustHaveHappenedOnceExactly();
        A.CallTo(() => _azureDevOpsPullRequestService.CountPullRequests(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    [Theory]
    [MemberData(nameof(ValidTerms))]
    public void IsMatch_ShouldReturnFalse_WhenServiceReturnsZeroCount(TermBase term)
    {
        // arrange
        A.CallTo(() => _azureDevOpsPullRequestService.CountPullRequests(_repository)).Returns(0);
        var sut = new HasPullRequestsMatcher(_azureDevOpsPullRequestService, true);

        // act
        var result = sut.IsMatch(_repository, term);

        // assert
        result.Should().BeFalse();
        A.CallTo(_azureDevOpsPullRequestService).MustHaveHappenedOnceExactly();
        A.CallTo(() => _azureDevOpsPullRequestService.CountPullRequests(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    [Theory]
    [MemberData(nameof(ValidTerms))]
    public void IsMatch_ShouldReturnFalse_WhenServiceThrows(TermBase term)
    {
        // arrange
        A.CallTo(() => _azureDevOpsPullRequestService.CountPullRequests(_repository)).Throws(new Exception("thrown by unit test"));
        var sut = new HasPullRequestsMatcher(_azureDevOpsPullRequestService, true);

        // act
        var result = sut.IsMatch(_repository, term);

        // assert
        result.Should().BeFalse();
        A.CallTo(_azureDevOpsPullRequestService).MustHaveHappenedOnceExactly();
        A.CallTo(() => _azureDevOpsPullRequestService.CountPullRequests(_repository)).MustHaveHappenedOnceExactly();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    public static IEnumerable<object[]> InvalidTerms
    {
        get
        {
            yield return [new TermRange("has", null, true, "x", false),];
            yield return [new SimpleTerm(string.Empty, string.Empty),];
            yield return [new SimpleTerm(string.Empty, "data"),];
            yield return [new SimpleTerm("is", "data"),];
            yield return [new SimpleTerm("has", "wrong-value"),];
        }
    }

    public static IEnumerable<object[]> ValidTerms
    {
        get
        {
            yield return [new SimpleTerm("has", "prs"),];
            yield return [new SimpleTerm("has", "pr"),];
            yield return [new SimpleTerm("has", "pull-request"),];
            yield return [new SimpleTerm("has", "pull-requests"),];
            yield return [new SimpleTerm("has", "pullrequest"),];
            yield return [new SimpleTerm("has", "pullrequests"),];
        }
    }
}