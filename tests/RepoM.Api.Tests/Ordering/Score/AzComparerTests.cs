namespace RepoM.Api.Tests.Ordering.Score;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Ordering.Score;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;
using Xunit;

public class ScoreComparerTests
{
    private readonly IRepository _repo1;
    private readonly IRepository _repo2;
    private readonly IRepositoryScoreCalculator _calculator;
    private readonly ScoreComparer _sut;

    public ScoreComparerTests()
    {
        _repo1 = A.Fake<IRepository>();
        _repo2 = A.Fake<IRepository>();
        _calculator = A.Fake<IRepositoryScoreCalculator>();
        _sut = new ScoreComparer(_calculator);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<ScoreComparer> act = () => new ScoreComparer(null!);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Compare_ShouldReturnZero_WhenBothAreNull()
    {
        // arrange

        // act
        var result = _sut.Compare(null!, null!);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Compare_ShouldReturnZero_WhenBothSameRepo()
    {
        // arrange

        // act
        var result = _sut.Compare(_repo1, _repo1);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Compare_ShouldReturnOne_WhenSecondRepoIsNull()
    {
        // arrange

        // act
        var result = _sut.Compare(_repo1, null);

        // assert
        result.Should().Be(1);
    }

    [Fact]
    public void Compare_ShouldReturnMinusOne_WhenSecondRepoIsNull()
    {
        // arrange

        // act
        var result = _sut.Compare(null, _repo1);

        // assert
        result.Should().Be(-1);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(10, 0, -10)]
    [InlineData(0, 10, 10)]
    [InlineData(7, 10, 3)]
    public void Compare_ShouldReturnSubtractedValueFromBothRepositoryScores(int score1, int score2, int expectedValue)
    {
        // arrange
        A.CallTo(() => _calculator.Score(_repo1)).Returns(score1);
        A.CallTo(() => _calculator.Score(_repo2)).Returns(score2);

        // act
        var result = _sut.Compare(_repo1, _repo2);

        // assert
        result.Should().Be(expectedValue);
    }
}