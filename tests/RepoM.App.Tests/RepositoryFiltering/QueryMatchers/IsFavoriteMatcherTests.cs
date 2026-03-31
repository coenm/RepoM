namespace RepoM.App.Tests.RepositoryFiltering.QueryMatchers;

using System;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Repositories.Favorite;
using Xunit;

public class IsFavoriteMatcherTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IFavoriteService _favoriteService = A.Fake<IFavoriteService>();
    private readonly IsFavoriteMatcher _sut;

    public IsFavoriteMatcherTests()
    {
        _sut = new IsFavoriteMatcher(_favoriteService);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<IsFavoriteMatcher> act = () => new IsFavoriteMatcher(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsMatch_ShouldReturnNull_WhenTermIsNotSimpleTerm()
    {
        // arrange
        TermBase term = A.Fake<TermBase>();

        // act
        var result = _sut.IsMatch(in _repository, in term);

        // assert
        result.Should().BeNull();
        A.CallTo(_favoriteService).MustNotHaveHappened();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("bla", "")]
    [InlineData("is", "")]
    [InlineData("", "bla")]
    [InlineData("x", "pinned")]
    [InlineData("", "pinned")]
    [InlineData("x", "unpinned")]
    [InlineData("", "unpinned")]
    [InlineData("is", "apinned")]
    [InlineData("is", "Pinned")] // invalid casing
    [InlineData("Is", "pinned")] // invalid casing
    [InlineData("is", "Unpinned")] // invalid casing
    [InlineData("Is", "unpinned")] // invalid casing
    public void IsMatch_ShouldReturnNull_WhenTermAndValueDoNotMatch(string term, string value)
    {
        // arrange
        TermBase simpleTerm = new SimpleTerm(term, value);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeNull();
        A.CallTo(_favoriteService).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsMatch_ShouldReturnIsFavoriteValue_WhenTermIsFavorite(bool isFavorite)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _favoriteService.IsFavorite("/safe/path")).Returns(isFavorite);
        TermBase simpleTerm = new SimpleTerm("is", "pinned");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().Be(isFavorite);
        A.CallTo(() => _favoriteService.IsFavorite("/safe/path")).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsMatch_ShouldReturnNegatedIsFavoriteValue_WhenTermIsUnpinned(bool isFavorite)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _favoriteService.IsFavorite("/safe/path")).Returns(isFavorite);
        TermBase simpleTerm = new SimpleTerm("is", "unpinned");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().Be(!isFavorite);
        A.CallTo(() => _favoriteService.IsFavorite("/safe/path")).MustHaveHappenedOnceExactly();
    }
}
