namespace RepoM.App.Tests.RepositoryFiltering.QueryMatchers;

using FakeItEasy;
using FluentAssertions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class IsBareRepositoryMatcherTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IsBareRepositoryMatcher _sut = new();

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
    [InlineData("", "bla")]
    [InlineData("x", "bare")]
    [InlineData("", "bare")]
    [InlineData("is", "abare")]
    [InlineData("is", "Bare")] // invalid casing
    [InlineData("Is", "bare")] // invalid casing
    public void IsMatch_ShouldReturnNull_WhenTermAndValueAreNotIsBare(string term, string value)
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
    public void IsMatch_ShouldReturnRepositoryBareValue_WhenTermAndValueMatches(bool repositoryIsBare)
    {
        // arrange
        A.CallTo(() => _repository.IsBare).Returns(repositoryIsBare);
        TermBase simpleTerm = new SimpleTerm("is", "bare");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().Be(repositoryIsBare);
        A.CallTo(() => _repository.IsBare).MustHaveHappenedOnceExactly();
    }
}