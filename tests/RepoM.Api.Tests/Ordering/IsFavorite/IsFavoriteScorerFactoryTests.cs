namespace RepoM.Api.Tests.Ordering.IsFavorite;

using System;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.Api.Ordering.IsFavorite;
using RepoM.Core.Repositories.Favorite;
using Xunit;

public class IsFavoriteScorerFactoryTests
{
    private readonly IFavoriteService _favoriteService = A.Fake<IFavoriteService>();
    private readonly IsFavoriteScorerFactory _sut;

    public IsFavoriteScorerFactoryTests()
    {
        _sut = new IsFavoriteScorerFactory(_favoriteService);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenFavoriteServiceIsNull()
    {
        // act
        Func<IsFavoriteScorerFactory> act = () => new IsFavoriteScorerFactory(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldReturnIsFavoriteScoreCalculator()
    {
        // arrange
        var config = new IsFavoriteScorerConfigurationV1 { Weight = 10, };

        // act
        var result = _sut.Create(config);

        // assert
        result.Should().NotBeNull();
        result.Should().BeOfType<IsFavoriteScoreCalculator>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-3)]
    [InlineData(100)]
    public void Create_ShouldPassWeightToCalculator(int weight)
    {
        // arrange
        var config = new IsFavoriteScorerConfigurationV1 { Weight = weight, };
        A.CallTo(() => _favoriteService.IsFavorite(A<string>._)).Returns(true);
        var repo = A.Fake<RepoM.Core.Plugin.Repository.IRepository>();
        A.CallTo(() => repo.SafePath).Returns("/test");

        // act
        var calculator = _sut.Create(config);
        var score = calculator.Score(repo);

        // assert
        score.Should().Be(weight);
    }
}
