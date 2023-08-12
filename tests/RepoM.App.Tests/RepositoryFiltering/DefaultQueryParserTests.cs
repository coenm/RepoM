namespace RepoM.App.Tests.RepositoryFiltering;

using FluentAssertions;
using RepoM.App.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using Xunit;

public class DefaultQueryParserTests
{
    private readonly DefaultQueryParser _sut;

    public DefaultQueryParserTests()
    {
        _sut = new DefaultQueryParser();
    }

    [Fact]
    public void Name_ShouldBeDefault()
    {
        _sut.Name.Should().Be("Default");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("abc def")]
    [InlineData("abc GHI")]
    [InlineData(" abc GHI ")]
    public void Parse_ShouldReturnFreeTextQuery_WhenInputHasNoSpecialMeaning(string input)
    {
        // arrange

        // act
        IQuery result = _sut.Parse(input);

        // assert
        result.Should().BeOfType<FreeText>();
        ((FreeText)result).Value.Should().Be(input);
    }

    [Theory]
    [InlineData("is:pinned", "pinned")]
    [InlineData("is:pinned  ", "pinned")]
    [InlineData("is:unpinned", "unpinned")]
    [InlineData("is:unpinned  ", "unpinned")]
    public void Parse_ShouldReturnSimpleTerm_WhenInputIsExactPinnedOrUnpinned(string input, string expectedValue)
    {
        // arrange

        // act
        IQuery result = _sut.Parse(input);

        // assert
        result.Should().BeOfType<SimpleTerm>();
        ((SimpleTerm)result).Term.Should().Be("is");
        ((SimpleTerm)result).Value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData("is:pinned test123")]
    [InlineData("is:pinned  test123")]
    [InlineData("is:unpinned test123")]
    [InlineData("is:unpinned  test123")]
    public void Parse_ShouldReturnAndQuery_WhenInputIsExactPinnedOrUnpinned(string input)
    {
        // arrange

        // act
        IQuery result = _sut.Parse(input);

        // assert
        result.Should().BeOfType<AndQuery>();
    }
}