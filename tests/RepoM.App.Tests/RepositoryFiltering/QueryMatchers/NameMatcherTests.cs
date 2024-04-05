namespace RepoM.App.Tests.RepositoryFiltering.QueryMatchers;

using FakeItEasy;
using FluentAssertions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class NameMatcherTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly NameMatcher _sut = new();

    [Fact]
    public void IsMatch_ShouldReturnNull_WhenTermIsWrongTermType()
    {
        // arrange
        TermBase term = A.Fake<TermBase>();

        // act
        var result = _sut.IsMatch(in _repository, in term);

        // assert
        result.Should().BeNull();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    public static TheoryData<string> InvalidTerms => new()
        {
            "name x",
            "naMe",
            "Name",
            " name",
            "Dummy",
        };

    [Theory]
    [MemberData(nameof(InvalidTerms))]
    public void IsMatch_ShouldReturnNull_WhenSimpleTermIsNotName(string term)
    {
        // arrange
        TermBase simpleTerm = new SimpleTerm(term, "dummy");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeNull();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    [Theory]
    [MemberData(nameof(InvalidTerms))]
    public void IsMatch_ShouldReturnNull_WhenStartsWithTermIsNotName(string term)
    {
        // arrange
        TermBase simpleTerm = new StartsWithTerm(term, "dummy");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeNull();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    public static TheoryData<string> InvalidNames => new()
        {
            "Repo M",
            "RepoM ",
            "repom",
            "Repom",
            "rep",
            "repo",
        };

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void IsMatch_ShouldReturnFalse_WhenSimpleTermIsNotExactlyAsName(string searchTerm)
    {
        // arrange
        A.CallTo(() => _repository.Name).Returns("RepoM");
        TermBase simpleTerm = new SimpleTerm("name", searchTerm);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeFalse();
        A.CallTo(() => _repository.Name).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IsMatch_ShouldReturnTrue_WhenSimpleTermIsExactlyAsName()
    {
        // arrange
        A.CallTo(() => _repository.Name).Returns("RepoM");
        TermBase simpleTerm = new SimpleTerm("name", "RepoM");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeTrue();
        A.CallTo(() => _repository.Name).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [MemberData(nameof(InvalidNames))]
    public void IsMatch_ShouldReturnFalse_WhenStartsWithTermIsNotExactlyAsName(string searchTerm)
    {
        // arrange
        A.CallTo(() => _repository.Name).Returns("RepoM");
        TermBase simpleTerm = new StartsWithTerm("name", searchTerm);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeFalse();
        A.CallTo(() => _repository.Name).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("RepoM")]
    [InlineData("Repo")]
    [InlineData("Re")]
    public void IsMatch_ShouldReturnTrue_WhenStartsWithTermIsAsName(string searchTerm)
    {
        // arrange
        A.CallTo(() => _repository.Name).Returns("RepoM");
        TermBase simpleTerm = new StartsWithTerm("name", searchTerm);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeTrue();
        A.CallTo(() => _repository.Name).MustHaveHappenedOnceExactly();
    }
}