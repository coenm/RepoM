namespace RepoM.App.Tests.RepositoryFiltering.QueryMatchers;

using FakeItEasy;
using FluentAssertions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class IsBehindMatcherTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IsBehindMatcher _sut = new();

    [Fact]
    public void IsMatch_ShouldReturnNull_WhenTermIsNotSimpleTerm()
    {
        // arrange
        TermBase term = A.Fake<TermBase>();

        // act
        var result = _sut.IsMatch(in _repository, in term);

        // assert
        result.Should().BeNull();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("","")]
    [InlineData("bla","")]
    [InlineData("is","")]
    [InlineData("", "bla")]
    [InlineData("x", "behind")]
    [InlineData("", "behind")]
    [InlineData("is", "abehind")]
    [InlineData("is", "Behind")] // invalid casing
    [InlineData("Is", "behind")] // invalid casing
    public void IsMatch_ShouldReturnNull_WhenTermAndValueAreNotIsBehind(string term, string value)
    {
        // arrange
        TermBase simpleTerm = new SimpleTerm(term, value);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeNull();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsMatch_ShouldReturnRepositoryIsBehindValue_WhenTermAndValueMatches(bool repositoryIsBehind)
    {
        // arrange
        A.CallTo(() => _repository.IsBehind).Returns(repositoryIsBehind);
        TermBase simpleTerm = new SimpleTerm("is", "behind");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().Be(repositoryIsBehind);
        A.CallTo(() => _repository.IsBehind).MustHaveHappenedOnceExactly();
    }
}