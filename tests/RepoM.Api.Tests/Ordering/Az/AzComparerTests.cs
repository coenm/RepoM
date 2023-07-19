namespace RepoM.Api.Tests.Ordering.Az;

using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Ordering.Az;
using RepoM.Core.Plugin.Repository;
using Xunit;

public class AzComparerTests
{
    private readonly IRepository _repo1;
    private readonly IRepository _repo2;

    public AzComparerTests()
    {
        _repo1 = A.Fake<IRepository>();
        _repo2 = A.Fake<IRepository>();
    }

    [Fact]
    public void Compare_ShouldBeZero_WhenReposAreNull()
    {
        // arrange
        var sut = new AzComparer(10, "Name");

        // act
        var result = sut.Compare(null, null);

        // assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(24)]
    public void Compare_ShouldBeWeight_WhenSecondRepoIsNull(int weight)
    {
        // arrange
        var sut = new AzComparer(weight, "Name");

        // act
        var result = sut.Compare(_repo1, null);

        // assert
        result.Should().Be(weight);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(24)]
    public void Compare_ShouldBeNegativeWeight_WhenFirstRepoIsNull(int weight)
    {
        // arrange
        var sut = new AzComparer(weight, "Name");

        // act
        var result = sut.Compare(null, _repo2);

        // assert
        result.Should().Be(weight * -1);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(24)]
    public void Compare_ShouldBeNegativeWeight_WhenNameOfFirstRepoIsBeforeNameSecondRepo(int weight)
    {
        // arrange
        A.CallTo(() => _repo1.Name).Returns("Abcd");
        A.CallTo(() => _repo2.Name).Returns("Def");
        var sut = new AzComparer(weight, "Name");

        // act
        var result = sut.Compare(_repo1, _repo2);

        // assert
        A.CallTo(() => _repo1.Name).MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo2.Name).MustHaveHappenedOnceExactly();
        result.Should().Be(weight * -1);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(24)]
    public void Compare_ShouldBeNegativeWeight_WhenNameOfFirstRepoIsAfterNameSecondRepo(int weight)
    {
        // arrange
        A.CallTo(() => _repo1.Name).Returns("Def");
        A.CallTo(() => _repo2.Name).Returns("Abcd");
        var sut = new AzComparer(weight, "Name");

        // act
        var result = sut.Compare(_repo1, _repo2);

        // assert
        A.CallTo(() => _repo1.Name).MustHaveHappenedOnceExactly();
        A.CallTo(() => _repo2.Name).MustHaveHappenedOnceExactly();
        result.Should().Be(weight);
    }
}