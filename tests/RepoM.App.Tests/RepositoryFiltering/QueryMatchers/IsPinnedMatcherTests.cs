namespace RepoM.App.Tests.RepositoryFiltering.QueryMatchers;

using System;
using FakeItEasy;
using AwesomeAssertions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Repositories.Pinning;
using Xunit;

public class IsPinnedMatcherTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly IPinningService _pinningService = A.Fake<IPinningService>();
    private readonly IsPinnedMatcher _sut;

    public IsPinnedMatcherTests()
    {
        _sut = new IsPinnedMatcher(_pinningService);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange

        // act
        Func<IsPinnedMatcher> act = () => new IsPinnedMatcher(null!);

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
        A.CallTo(_pinningService).MustNotHaveHappened();
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
        A.CallTo(_pinningService).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsMatch_ShouldReturnIsPinnedValue_WhenTermIsPinned(bool isPinned)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _pinningService.IsPinned("/safe/path")).Returns(isPinned);
        TermBase simpleTerm = new SimpleTerm("is", "pinned");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().Be(isPinned);
        A.CallTo(() => _pinningService.IsPinned("/safe/path")).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsMatch_ShouldReturnNegatedIsPinnedValue_WhenTermIsUnpinned(bool isPinned)
    {
        // arrange
        A.CallTo(() => _repository.SafePath).Returns("/safe/path");
        A.CallTo(() => _pinningService.IsPinned("/safe/path")).Returns(isPinned);
        TermBase simpleTerm = new SimpleTerm("is", "unpinned");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().Be(!isPinned);
        A.CallTo(() => _pinningService.IsPinned("/safe/path")).MustHaveHappenedOnceExactly();
    }
}
