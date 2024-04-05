namespace RepoM.App.Tests.RepositoryFiltering.QueryMatchers;

using FakeItEasy;
using FluentAssertions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class TagMatcherTests
{
    private readonly IRepository _repository = A.Fake<IRepository>();
    private readonly TagMatcher _sut = new();

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
            "tag x",
            "Tag",
            "TaG",
            " tag",
            "Dummy",
        };

    [Theory]
    [MemberData(nameof(InvalidTerms))]
    public void IsMatch_ShouldReturnNull_WhenSimpleTermIsNotTag(string term)
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
    public void IsMatch_ShouldReturnNull_WhenStartsWithTermIsNotTag(string term)
    {
        // arrange
        TermBase simpleTerm = new StartsWithTerm(term, "dummy");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeNull();
        A.CallTo(_repository).MustNotHaveHappened();
    }

    public static TheoryData<string> WrongTags => new()
        {
            "Repo M",
            "RepoM ",
            "rep",
            "repo",
        };

    [Theory]
    [MemberData(nameof(WrongTags))]
    public void IsMatch_ShouldReturnFalse_WhenSimpleTermIsNotAsTag(string searchTerm)
    {
        // arrange
        A.CallTo(() => _repository.Tags).Returns(["RepoM", "WORK",]);
        TermBase simpleTerm = new SimpleTerm("tag", searchTerm);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeFalse();
        A.CallTo(() => _repository.Tags).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IsMatch_ShouldReturnTrue_WhenSimpleTermIsExactlyAsName()
    {
        // arrange
        A.CallTo(() => _repository.Tags).Returns(["RepoM", "WORK",]);
        TermBase simpleTerm = new SimpleTerm("tag", "RepoM");

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeTrue();
        A.CallTo(() => _repository.Tags).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [MemberData(nameof(WrongTags))]
    public void IsMatch_ShouldReturnFalse_WhenStartsWithTermIsNotExactlyAsName(string searchTerm)
    {
        // arrange
        A.CallTo(() => _repository.Tags).Returns(["RepoM", "WORK",]);
        TermBase simpleTerm = new StartsWithTerm("tag", searchTerm);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeFalse();
        A.CallTo(() => _repository.Tags).MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("RepoM")]
    [InlineData("Repo")]
    [InlineData("Re")]
    public void IsMatch_ShouldReturnTrue_WhenStartsWithTermIsAsName(string searchTerm)
    {
        // arrange
        A.CallTo(() => _repository.Tags).Returns(["RepoM", "WORK",]);
        TermBase simpleTerm = new StartsWithTerm("tag", searchTerm);

        // act
        var result = _sut.IsMatch(in _repository, in simpleTerm);

        // assert
        result.Should().BeTrue();
        A.CallTo(() => _repository.Tags).MustHaveHappenedOnceExactly();
    }
}