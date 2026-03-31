namespace RepoM.Api.Tests.Ordering.IsFavorite;

using System;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.Api.Ordering.IsFavorite;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Favorite;
using Xunit;

public class IsFavoriteScoreCalculatorTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IFavoriteService _favoriteService = A.Fake<IFavoriteService>();

    [Fact]
    public void Ctor_ShouldThrow_WhenFavoriteServiceIsNull()
    {
        // arrange

        // act
        Func<IsFavoriteScoreCalculator> act = () => new IsFavoriteScoreCalculator(null!, 10);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(-5)]
    [InlineData(100)]
    public void Score_ShouldReturnWeight_WhenRepositoryIsFavorite(int weight)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _favoriteService.IsFavorite("/safe/path")).Returns(true);
        var sut = new IsFavoriteScoreCalculator(_favoriteService, weight);

        // act
        var result = sut.Score(_repository);

        // assert
        result.Should().Be(weight);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(-5)]
    [InlineData(100)]
    public void Score_ShouldReturnZero_WhenRepositoryIsNotFavorite(int weight)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _favoriteService.IsFavorite("/safe/path")).Returns(false);
        var sut = new IsFavoriteScoreCalculator(_favoriteService, weight);

        // act
        var result = sut.Score(_repository);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Score_ShouldUseSafePathFromRepository()
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/specific/repo/path");
        A.CallTo(() => _favoriteService.IsFavorite(A<string>._)).Returns(false);
        var sut = new IsFavoriteScoreCalculator(_favoriteService, 5);

        // act
        sut.Score(_repository);

        // assert
        A.CallTo(() => _favoriteService.IsFavorite("/specific/repo/path")).MustHaveHappenedOnceExactly();
        A.CallTo(_favoriteService).MustHaveHappenedOnceExactly();
    }
}
