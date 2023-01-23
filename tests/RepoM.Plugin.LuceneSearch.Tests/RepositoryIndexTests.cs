namespace RepoM.Plugin.LuceneSearch.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using RepoM.Plugin.LuceneSearch;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class RepositoryIndexTests
{
    [Fact]
    public void Test1()
    {
        // arrange
        var sut = new RepositoryIndex(new LuceneDirectoryInstance(new RamLuceneDirectoryFactory()));

        // act
        _ = sut.Search("tag:work project x", SearchOperator.Or, out var hits);

        // assert
        hits.Should().Be(0);
    }

    [Fact]
    public async Task Tesdt1()
    {
        // arrange
        var sut = new RepositoryIndex(new LuceneDirectoryInstance(new RamLuceneDirectoryFactory()));
        var item = new RepositorySearchModel(
            "c:/a/b/c",
            "RepoM",
            new List<string>()
                {
                    "repositories",
                    "work",
                });
        await sut.ReIndexMediaFileAsync(item).ConfigureAwait(false);

        // act
        _ = sut.Search("tag:work project x", SearchOperator.Or, out var hits);

        // assert
        hits.Should().Be(1);
    }

    [Fact]
    public async Task ExperimentalTests()
    {
        // arrange
        var analyzer = new WhitespaceAnalyzer(LuceneNetVersion.VERSION);
        // var analyzer = new KeywordAnalyzer();

        // act
        var queryParser = new MultiFieldQueryParser(LuceneNetVersion.VERSION, new[] { "free-text", }, analyzer)
            {
                DefaultOperator = Operator.AND,
            };

        Query? result = queryParser.Parse("tag:work test1 age:[41 TO 51] DataRontonde Core");

        MyQueryBase x = MapQuery(result);

        // assert
        await Verifier.Verify(new
                          {
                              Lucene = result,
                              RepoM = x,
                          })
                      .IgnoreMember("Bytes")
                      .IgnoreMember("Boost")
                      .IgnoreMember("IsProhibited");
    }

    private MyQueryBase MapQuery(Query query)
    {
        if (query is Lucene.Net.Search.BooleanQuery bq)
        {
            return new MyBooleanQuery(bq, MapQuery);
        }

        if (query is Lucene.Net.Search.TermQuery tq)
        {
            return new MyTermQuery(tq);
        }

        if (query is Lucene.Net.Search.TermRangeQuery trq)
        {
            return new MyTermRangeQuery(trq);
        }

        var fullName = query.GetType().FullName;
        throw new NotImplementedException(fullName);
    }
}

public sealed class MyTerm
{
    public string Field { get; init; }
    public string Text { get; init;  }
}

public class MyBooleanClause
{
    // LUCENENET specific - de-nested Occur from BooleanClause in order to prevent
    // a naming conflict with the Occur property

    public static string ToString(Occur occur)
    {
        switch (occur)
        {
            case Occur.MUST:
                return "+";

            case Occur.SHOULD:
                return "";

            case Occur.MUST_NOT:
                return "-";

            default:
                throw new ArgumentOutOfRangeException("Invalid Occur value"); // LUCENENET specific
        }
    }

    /// <summary>
    /// The query whose matching documents are combined by the boolean query.
    /// </summary>
    private MyQueryBase query;

    private Occur occur;

    /// <summary>
    /// Constructs a <see cref="BooleanClause"/>.
    /// </summary>
    public MyBooleanClause(MyQueryBase query, Occur occur)
    {
        this.query = query;
        this.occur = occur;
    }

    public virtual Occur Occur
    {
        get => occur;
        set => occur = value;
    }

    public virtual MyQueryBase Query
    {
        get => query;
        set => query = value;
    }

    public virtual bool IsProhibited => Occur.MUST_NOT == occur;

    public virtual bool IsRequired => Occur.MUST == occur;
}

/// <summary>
/// Specifies how clauses are to occur in matching documents. </summary>
public enum Occur
{
    /// <summary>
    /// Use this operator for clauses that <i>must</i> appear in the matching documents.
    /// </summary>
    MUST,

    /// <summary>
    /// Use this operator for clauses that <i>should</i> appear in the
    /// matching documents. For a <see cref="BooleanQuery"/> with no <see cref="MUST"/>
    /// clauses one or more <see cref="SHOULD"/> clauses must match a document
    /// for the <see cref="BooleanQuery"/> to match. </summary>
    /// <seealso cref="BooleanQuery.MinimumNumberShouldMatch"/>
    SHOULD,

    /// <summary>
    /// Use this operator for clauses that <i>must not</i> appear in the matching documents.
    /// Note that it is not possible to search for queries that only consist
    /// of a <see cref="MUST_NOT"/> clause.
    /// </summary>
    MUST_NOT
}

public abstract class MyQueryBase
{

}

public class MyTermRangeQuery : MyQueryBase
{
    public MyTermRangeQuery(Lucene.Net.Search.TermRangeQuery inner)
    {
        IncludesLower = inner.IncludesLower;
        IncludesUpper = inner.IncludesUpper;
        Field = inner.Field;
        
        LowerText = Term.ToString(inner.LowerTerm);
        UpperText = Term.ToString(inner.UpperTerm);
    }

    /// <summary>
    /// Returns <c>true</c> if the lower endpoint is inclusive </summary>
    public bool IncludesLower { get; }

    public string Field { get; }

    public string LowerText { get; }

    public string UpperText { get; }

    /// <summary>
    /// Returns <c>true</c> if the upper endpoint is inclusive </summary>
    public bool IncludesUpper { get; }
}

public class MyTermQuery : MyQueryBase
{
    internal MyTermQuery(TermQuery termQuery)
    {
        Term = new MyTerm
            {
                Field = termQuery.Term.Field,
                Text = termQuery.Term.Text,
            };
    }

    public MyTerm Term { get; init; }
}

public class MyBooleanQuery : MyQueryBase
{
    private readonly List<MyBooleanClause> _clauses;

    internal MyBooleanQuery(Lucene.Net.Search.BooleanQuery bq, Func<Query, MyQueryBase> map)
    {
        _clauses = bq.Clauses
                     .Select(x => new MyBooleanClause(map(x.Query), MapOccur(x.Occur)))
                     .ToList();


    }

    public void Add(MyBooleanClause clause)
    {
        _clauses.Add(clause);
    }

    public IReadOnlyList<MyBooleanClause> Clauses => _clauses.AsReadOnly();

    private static Occur MapOccur(Lucene.Net.Search.Occur clauseOccur)
    {
        return clauseOccur switch
            {
                Lucene.Net.Search.Occur.MUST => Occur.MUST,
                Lucene.Net.Search.Occur.SHOULD => Occur.SHOULD,
                Lucene.Net.Search.Occur.MUST_NOT => Occur.MUST_NOT,
                _ => throw new ArgumentOutOfRangeException(nameof(clauseOccur), clauseOccur, null)
            };
    }
}