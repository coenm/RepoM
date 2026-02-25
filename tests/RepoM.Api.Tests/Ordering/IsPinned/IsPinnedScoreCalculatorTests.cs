namespace RepoM.Api.Tests.Ordering.IsPinned;

using System;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.Api.Ordering.IsPinned;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Pinning;
using Xunit;

public class IsPinnedScoreCalculatorTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IPinningService _pinningService = A.Fake<IPinningService>();

    [Fact]
    public void Ctor_ShouldThrow_WhenPinningServiceIsNull()
    {
        // arrange

        // act
        Func<IsPinnedScoreCalculator> act = () => new IsPinnedScoreCalculator(null!, 10);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(-5)]
    [InlineData(100)]
    public void Score_ShouldReturnWeight_WhenRepositoryIsPinned(int weight)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _pinningService.IsPinned("/safe/path")).Returns(true);
        var sut = new IsPinnedScoreCalculator(_pinningService, weight);

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
    public void Score_ShouldReturnZero_WhenRepositoryIsNotPinned(int weight)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _pinningService.IsPinned("/safe/path")).Returns(false);
        var sut = new IsPinnedScoreCalculator(_pinningService, weight);

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
        A.CallTo(() => _pinningService.IsPinned(A<string>._)).Returns(false);
        var sut = new IsPinnedScoreCalculator(_pinningService, 5);

        // act
        sut.Score(_repository);

        // assert
        A.CallTo(() => _pinningService.IsPinned("/specific/repo/path")).MustHaveHappenedOnceExactly();
        A.CallTo(_pinningService).MustHaveHappenedOnceExactly();
    }
}
