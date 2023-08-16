namespace RepoM.Plugin.Statistics.Tests.Ordering;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Statistics.Ordering;
using Xunit;

public class LastOpenedComparerTests
{
    private readonly IStatisticsService _service;
    private readonly IRepository _repo1;
    private readonly IRepository _repo2;

    public LastOpenedComparerTests()
    {
        _service = A.Fake<IStatisticsService>();
        _repo1 = A.Fake<IRepository>();
        _repo2 = A.Fake<IRepository>();
    }

    [Fact]
    public void Ctor_ShouldThrown_WhenArgumentIsNull()
    {
        // arrange

        // act
        Action act1 = () => _ = new LastOpenedComparer(null!, 10);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Compare_ShouldReturnZero_WhenWeightIsZero()
    {
        // arrange
        var sut = new LastOpenedComparer(_service, 0);

        // act
        var result = sut.Compare(A.Dummy<IRepository>(), A.Dummy<IRepository>());

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Compare_ShouldReturnZero_WhenRepositoriesAreSameObject()
    {
        // arrange
        var sut = new LastOpenedComparer(_service, 10);

        // act
        var result = sut.Compare(_repo1, _repo1);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Compare_ShouldReturnNegativeWeight_WhenFirstRepositoryIsNull()
    {
        // arrange
        var sut = new LastOpenedComparer(_service, 10);

        // act
        var result = sut.Compare(null!, _repo1);

        // assert
        result.Should().Be(-10);
    }

    [Fact]
    public void Compare_ShouldReturnWeight_WhenSecondRepositoryIsNull()
    {
        // arrange
        var sut = new LastOpenedComparer(_service, 10);

        // act
        var result = sut.Compare(_repo1, null!);

        // assert
        result.Should().Be(10);
    }

    [Fact]
    public void Compare_ShouldReturnNegativeWeight_WhenFirstRepoIsGreaterThenSecondRepo()
    {
        // arrange
        SetupRecordings(_repo1, DateTime.Now.AddDays(-1));
        SetupRecordings(_repo2, DateTime.Now.AddDays(-2));

        var sut = new LastOpenedComparer(_service, 10);

        // act
        var result = sut.Compare(_repo1, _repo2);

        // assert
        result.Should().Be(-10);
    }

    [Fact]
    public void Compare_ShouldReturnWeight_WhenFirstRepoIsLessThenSecondRepo()
    {
        // arrange
        SetupRecordings(_repo1, DateTime.Now.AddDays(-1));
        SetupRecordings(_repo2, DateTime.Now.AddDays(-2));
        var sut = new LastOpenedComparer(_service, 10);

        // act
        var result = sut.Compare(_repo2, _repo1);

        // assert
        result.Should().Be(10);
    }

    [Fact]
    public void Compare_ShouldReturnWeight_WhenSecondHasNoTimestamps()
    {
        // arrange
        SetupRecordings(_repo1, DateTime.Now.AddDays(-1));
        SetupRecordings(_repo2);
        var sut = new LastOpenedComparer(_service, 10);

        // act
        var result = sut.Compare(_repo2, _repo1);

        // assert
        result.Should().Be(10);
    }

    [Fact]
    public void Compare_ShouldReturnZero_WhenBothHaveSameDateTime()
    {
        // arrange
        DateTime dt = DateTime.Now.AddDays(-1);
        SetupRecordings(_repo1, dt);
        SetupRecordings(_repo2, dt);
        var sut = new LastOpenedComparer(_service, 10);

        // act
        var result = sut.Compare(_repo2, _repo1);

        // assert
        result.Should().Be(0);
    }

    private void SetupRecordings(IRepository repo, params DateTime[] recordings)
    {
        A.CallTo(() => _service.GetRecordings(repo)).Returns(recordings);
    }
}