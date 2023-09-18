namespace RepoM.App.Tests.RepositoryFiltering;

using System;
using FakeItEasy;
using FluentAssertions;
using RepoM.App.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using Xunit;

public class QueryParserCompositionTests
{
    private readonly INamedQueryParser _qp1 = A.Fake<INamedQueryParser>();
    private readonly INamedQueryParser _qp2 = A.Fake<INamedQueryParser>();
    private readonly INamedQueryParser _qp3 = A.Fake<INamedQueryParser>();
    private readonly IQuery _query1 = A.Fake<IQuery>();
    private readonly IQuery _query2 = A.Fake<IQuery>();
    private readonly IQuery _query3 = A.Fake<IQuery>();
    private readonly QueryParserComposition _sut;

    public QueryParserCompositionTests()
    {
        _sut = new QueryParserComposition(new[] { _qp1, _qp2, _qp3, });

        A.CallTo(() => _qp1.Name).Returns("key1");
        A.CallTo(() => _qp2.Name).Returns("key2");
        A.CallTo(() => _qp3.Name).Returns("key3");

        A.CallTo(() => _qp1.Parse(A<string>._)).Returns(_query1);
        A.CallTo(() => _qp2.Parse(A<string>._)).Returns(_query2);
        A.CallTo(() => _qp3.Parse(A<string>._)).Returns(_query3);
    }

    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentsAreEmpty()
    {
        // arrange

        // act
        Action act = () => _ = new QueryParserComposition(Array.Empty<INamedQueryParser>());

        // assert
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Parse_ShouldUseFirstQueryParser_ByDefault()
    {
        // arrange

        // act
        _ = _sut.Parse("text");

        // assert
        A.CallTo(() => _qp1.Parse("text")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _qp2.Parse(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _qp3.Parse(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Parse_ShouldUsedCachedResult_WhenQueryIsSame()
    {
        // arrange

        // act
        _ = _sut.Parse("text");
        _ = _sut.Parse("text");

        // assert
        A.CallTo(() => _qp1.Parse("text")).MustHaveHappenedOnceExactly();
        A.CallTo(() => _qp2.Parse(A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _qp3.Parse(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public void Parse_ShouldReturnResultFromNamedQueryParser()
    {
        // arrange
        A.CallTo(() => _qp1.Parse(A<string>._)).Returns(_query1);

        // act
        IQuery result = _sut.Parse("text");

        // assert
        _ = result.Should().Be(_query1);
    }

    [Fact]
    public void SetComparer_ShouldSwitchComparer_WhenKeyExists()
    {
        // arrange
        
        // act
        _sut.SetComparer("key2");

        // assert
        IQuery result = _sut.Parse("text");
        result.Should().Be(_query2);
    }

    [Fact]
    public void SetComparer_ShouldNotSwitchComparer_WhenKeyNotExists()
    {
        // arrange

        // act
        _sut.SetComparer("key2dummy");

        // assert
        IQuery result = _sut.Parse("text");
        result.Should().Be(_query1);
    }
}