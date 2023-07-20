namespace RepoM.Api.Tests.Ordering.Composition;

using FakeItEasy;
using FluentAssertions;
using RepoM.Api.Ordering.Composition;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;
using Xunit;

public class CompositionComparerTests
{
    private readonly IRepository _repo1;
    private readonly IRepository _repo2;

    private readonly IRepositoryComparer _comparer1;
    private readonly IRepositoryComparer _comparer2;
    private readonly IRepositoryComparer _comparer3;
    private readonly CompositionComparer _sut;

    public CompositionComparerTests()
    {
        _repo1 = A.Fake<IRepository>();
        _repo2 = A.Fake<IRepository>();
        _comparer1 = A.Fake<IRepositoryComparer>();
        _comparer2 = A.Fake<IRepositoryComparer>();
        _comparer3 = A.Fake<IRepositoryComparer>();
        _sut = new CompositionComparer(new[] { _comparer1, _comparer2, _comparer3, });
    }

    [Fact]
    public void Compare_ShouldReturnZero_WhenReposAreNull()
    {
        // arrange

        // act
        var result = _sut.Compare(null, null);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Compare_ShouldReturnZero_WhenRepositoriesAreSame()
    {
        // arrange

        // act
        var result = _sut.Compare(_repo1, _repo1);

        // assert
        result.Should().Be(0);
    }

    [Fact]
    public void Compare_ShouldReturnWeight_WhenSecondRepoIsNull()
    {
        // arrange

        // act
        var result = _sut.Compare(_repo1, null);

        // assert
        result.Should().Be(1);
    }

    [Fact]
    public void Compare_ShouldReturnNegativeWeight_WhenFirstRepoIsNull()
    {
        // arrange

        // act
        var result = _sut.Compare(null, _repo2);

        // assert
        result.Should().Be(-1);
    }

    [Theory]
    [InlineData(-15)]
    [InlineData(5)]
    public void Compare_ShouldReturnFirstValue_WhenFirstComparerReturnsSomethingElseThenZero(int returnValue)
    {
        // arrange
        A.CallTo(() => _comparer1.Compare(_repo1, _repo2)).Returns(returnValue);

        // act
        var result = _sut.Compare(_repo1, _repo2);

        // assert
        result.Should().Be(returnValue);
        A.CallTo(() => _comparer1.Compare(_repo1, _repo2)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _comparer2.Compare(_repo1, _repo2)).MustNotHaveHappened();
        A.CallTo(() => _comparer3.Compare(_repo1, _repo2)).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(-45)]
    [InlineData(35)]
    public void Compare_ShouldReturnValueOfSecondComparer_WhenFirstComparerReturnedZero(int returnValue)
    {
        // arrange
        A.CallTo(() => _comparer1.Compare(_repo1, _repo2)).Returns(0);
        A.CallTo(() => _comparer2.Compare(_repo1, _repo2)).Returns(returnValue);

        // act
        var result = _sut.Compare(_repo1, _repo2);

        // assert
        result.Should().Be(returnValue);
        A.CallTo(() => _comparer1.Compare(_repo1, _repo2)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _comparer2.Compare(_repo1, _repo2)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _comparer3.Compare(_repo1, _repo2)).MustNotHaveHappened();
    }

    [Fact]
    public void Compare_ShouldReturnZero_WhenAllComparersReturnZero()
    {
        // arrange
        A.CallTo(() => _comparer1.Compare(_repo1, _repo2)).Returns(0);
        A.CallTo(() => _comparer2.Compare(_repo1, _repo2)).Returns(0);
        A.CallTo(() => _comparer3.Compare(_repo1, _repo2)).Returns(0);

        // act
        var result = _sut.Compare(_repo1, _repo2);

        // assert
        result.Should().Be(0);
        A.CallTo(() => _comparer1.Compare(_repo1, _repo2)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _comparer2.Compare(_repo1, _repo2)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _comparer3.Compare(_repo1, _repo2)).MustHaveHappenedOnceExactly();
    }
}