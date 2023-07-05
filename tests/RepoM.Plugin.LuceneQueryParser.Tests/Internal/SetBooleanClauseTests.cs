namespace RepoM.Plugin.LuceneQueryParser.Tests.Internal;

using FluentAssertions;
using Lucene.Net.Search;
using RepoM.Plugin.LuceneQueryParser.Internal;
using Xunit;

public class SetBooleanClauseTests
{
    private readonly WrappedBooleanClause _wrappedBooleanClause;

    public SetBooleanClauseTests()
    {
        _wrappedBooleanClause = new WrappedBooleanClause(new BooleanClause(new BooleanQuery(), Occur.MUST));
    }

    [Fact]
    public void Mode_ShouldBeNothing_WhenInitialized()
    {
        // arrange
        
        var sut = new SetBooleanClause(_wrappedBooleanClause);

        // act
        var result = sut.Mode;

        // assert
        result.Should().Be(SetBooleanClause.BoolMode.Nothing);
    }

    [Fact]
    public void ToString_ShouldReturnToStringFromInnerClause_WhenOnlyOneClause()
    {
        // arrange
        var wrappedBooleanClause = new WrappedBooleanClause(new BooleanClause(new BooleanQuery(), Occur.MUST));
        var sut = new SetBooleanClause(wrappedBooleanClause);

        // act
        var result = sut.ToString();

        // assert
        result.Should().Be(wrappedBooleanClause.ToString());
    }

    [Fact]
    public void Ctor_ShouldAddArgumentInItemsCollection1()
    {
        // arrange
        var wrappedBooleanClause = new WrappedBooleanClause(new BooleanClause(new BooleanQuery(), Occur.MUST));
        var sut = new SetBooleanClause(wrappedBooleanClause);

        // act
        WrappedBooleanClause[] result = sut.Items.ToArray();

        // assert
        result.Should().BeEquivalentTo(new[] { wrappedBooleanClause, });
    }

    [Fact]
    public void Ctor_ShouldAddArgumentInItemsCollection2()
    {
        // arrange
        var wrappedBooleanClause1 = new WrappedBooleanClause(new BooleanClause(new BooleanQuery(), Occur.MUST));
        var wrappedBooleanClause2 = new WrappedBooleanClause(new BooleanClause(new BooleanQuery(), Occur.SHOULD));
        var sut = new SetBooleanClause(wrappedBooleanClause1, wrappedBooleanClause2);

        // act
        WrappedBooleanClause[] result = sut.Items.ToArray();

        // assert
        result.Should().BeEquivalentTo(new[] { wrappedBooleanClause1, wrappedBooleanClause2, });
    }
}