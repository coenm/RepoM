namespace RepoM.Plugin.LuceneSearch.Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using Argon;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using RepoM.Plugin.LuceneQueryParser.LuceneX;
using RepoM.Plugin.LuceneQueryParser.Plugin.Clause;
using RepoM.Plugin.LuceneQueryParser.Plugin.Clause.Terms;
using RepoM.Plugin.LuceneSearch;
using VerifyTests;
using VerifyXunit;
using Xunit;

// https://lucene.apache.org/core/2_9_4/queryparsersyntax.html
[UsesVerify]
public class RepositoryIndexTests
{
    private readonly WhitespaceAnalyzer _analyzer;
    private CustomMultiFieldQueryParser _queryParser;
    private readonly VerifySettings _settings;

    public RepositoryIndexTests()
    {
        _analyzer = new WhitespaceAnalyzer(LuceneNetVersion.VERSION);

        _queryParser = new CustomMultiFieldQueryParser(LuceneNetVersion.VERSION, new[] { "free-text", }, _analyzer)
            {
                DefaultOperator = Operator.AND,
                AllowLeadingWildcard = true,
                FuzzyMinSim = 1.0f, //FuzzyQuery 
                PhraseSlop = 0, // disable Proximity
            };

        _settings = new VerifySettings();
        _settings.AddExtraSettings(settings =>
            {
                settings.DefaultValueHandling = DefaultValueHandling.Include;
                // settings.NullValueHandling = NullValueHandling.Include;
                settings.TypeNameHandling = TypeNameHandling.Auto;

            });
        _settings.IgnoreMember("Bytes");
        _settings.IgnoreMember("Boost");
        _settings.IgnoreMember("IsProhibited");
        _settings.IgnoreMember("MultiTermRewriteMethod");
        _settings.IgnoreMember("Automaton");
        _settings.IgnoreMember("MaxEdits");
        _settings.DisableRequireUniquePrefix();
    }

    [Fact]
    public async Task Abc()
    {
        // arrange
        var input = "tag:drc";
        input = "tag:drc OR -tag:btw AND tag:3"; // (x OR (y AND z))
        input = "(tag:btw AND tag:3)";


        // act
        var result = (SetQuery)_queryParser.Parse(input);

        // assert
        await Verifier.Verify(new
                              {
                                  Output = result.ToString(),
                                  Lucene = result,
                                  RepoM = MapQuery2(result, _queryParser.DefaultOperator == Operator.AND),
                              },
                          _settings)
                      .AddExtraSettings(x =>
                          {
                              x.TypeNameHandling = TypeNameHandling.All;
                          });
    }

    private IQuery MapQuery2(SetQuery result, bool b)
    {
        return ConvertX(result.SetBooleanClause);
    }

    private IQuery ConvertX(WrappedBooleanClause input)
    {
        if (input is NotBooleanClause nbc)
        {
            return new NotQuery(ConvertQueryToClause(nbc.Query));
        }

        if (input is SetBooleanClause x)
        {
            if (x.Items.Count == 1)
            {
                return ConvertX(x.Items.Single());
            }

            IQuery[] array = x.Items.Select(booleanClause => ConvertX(booleanClause)).ToArray();

            return x.Mode == SetBooleanClause.BoolMode.AND
                ? new AndQuery(array)
                : new OrQuery(array);
        }

        //WrappedBooleanClause
        return ConvertQueryToClause(input.Query);
    }

    private IQuery ConvertQueryToClause(Query query)
    {
        // if (query is Lucene.Net.Search.BooleanQuery bq)
        // {
        //     // and, or
        //     TermBase[] items = bq.Clauses.Select(x =>
        //         {
        //             if (x.Occur == Lucene.Net.Search.Occur.MUST_NOT)
        //             {
        //                 return new Not(MapQuery1(x.Query, and));
        //             }
        //
        //             return MapQuery1(x.Query, and);
        //         }).ToArray();
        //
        //
        //     if (items.Length == 1)
        //     {
        //         return items.Single();
        //     }
        //
        //     return and ? new And(items) : new Or(items);
        //     // return new MyBooleanQuery(bq, MapQuery);
        // }
        //
        if (query is Lucene.Net.Search.TermQuery tq)
        {
            return new SimpleTerm(tq.Term.Field, tq.Term.Text);
        }
       
        if (query is Lucene.Net.Search.TermRangeQuery trq)
        {
            return new TermRange(
                trq.Field,
                Term.ToString(trq.LowerTerm),
                trq.IncludesLower,
                Term.ToString(trq.UpperTerm),
                trq.IncludesUpper);
        }
        
        if (query is Lucene.Net.Search.WildcardQuery wq)
        {
            return new WildCardTerm(wq.Term.Field, wq.Term.Text);
        }
        //
        // if (query is FuzzyQuery fq)
        // {
        //     // do not support fuzzy query,
        //     return new SimpleTerm(fq.Term.Field, fq.Term.Text);
        // }

        if (query is SetQuery cq)
        {
            // 
            return MapQuery2(cq, true);
        }


        var fullName = query.GetType().FullName;
        throw new NotImplementedException(fullName);
    }
    //
    //
    //
    // [Fact]
    // public void Test1()
    // {
    //     // arrange
    //     var sut = new RepositoryIndex(new LuceneDirectoryInstance(new RamLuceneDirectoryFactory()));
    //
    //     // act
    //     _ = sut.Search("tag:work project x", SearchOperator.Or, out var hits);
    //
    //     // assert
    //     hits.Should().Be(0);
    // }
    //
    // [Theory]
    // [InlineData("tag-only", "tag:abc")]
    // [InlineData("tag-only", "  tag:abc  ")]
    // [InlineData("tag-only", "tag:\"abc\"")]
    // [InlineData("tag-only", " tag:\"abc\"")]
    // [InlineData("tag-only", " tag:\"abc  \"")]
    // [InlineData("tag-only", " tag:\"   abc  \"")]
    // [InlineData("tag-only", " (tag:\"   abc  \")")]
    //
    // [InlineData("tag-plus", " +tag:\"   abc  \"")]
    // [InlineData("tag-plus", " +tag:abc")]
    // [InlineData("tag-plus", " (+tag:abc)")]
    // [InlineData("tag-plus", " +(tag:abc)")]
    // [InlineData("tag-plus", " (+(tag:abc))")]
    //
    // [InlineData("tag-min", " -tag:\"   abc  \"")]
    // [InlineData("tag-min", " -tag:abc")]
    // [InlineData("tag-min", " (-tag:abc)")]
    // [InlineData("tag-min", " -(tag:abc)")]
    //
    // [InlineData("single-word", "aBc@")]
    // [InlineData("single-word", " aBc@ ")]
    // [InlineData("single-word", " \"aBc@\" ")]
    // [InlineData("single-word", " ((\"aBc@\")) ")]
    // // [InlineData("single-word", " +(\"aBc@\") ")]
    // // [InlineData("single-word", " (+\"aBc@\") ")]
    //
    // // [InlineData("text-only", "This is Some   Text@ ")]
    // // [InlineData("text-only", "This is Some Text@")]
    // // [InlineData("text-only", "  This is Some Text@  ")]
    // // [InlineData("text-only", "  This is      Some Text@  ")]
    // // [InlineData("text-only", "  This is      Some^4 Text@  ")] // boosting ignored
    // // // [InlineData("text-only", "  This is      Some^ Text@  ")]  // error
    // // [InlineData("text-only", "  +This +is      Some Text@  ")] // plus doesnt matter
    // //
    // [InlineData("range-only", "age:[16 TO 75]")]
    // [InlineData("range-only", "  age:[16 TO 75]")]
    // [InlineData("range-only", "age:[16 TO 75]  ")]
    // [InlineData("range-only", "age:[16   TO   75]")]
    // [InlineData("range-only", "age:[  16 TO 75 ]")]
    // [InlineData("range-only", "age: [16 TO 75]")]
    // [InlineData("range-only", "age : [16 TO 75]")]
    // [InlineData("range-only", "age :[16 TO 75]")]
    // [InlineData("range-only", "  age:[16 TO 75] ")]
    // [InlineData("range-only", "(age:[16 TO 75])")]
    // [InlineData("range-only-must", "+(age:[16 TO 75])")]
    // [InlineData("range-only-must", "(+age:[16 TO 75])")]
    // [InlineData("range-only-excl-left", "age:{16 TO 75]")]
    // [InlineData("range-only-excl-right", "age:[16 TO 75}")]
    // [InlineData("range-only-excl", "age:{16 TO 75}")]
    //
    // // // wildcard not yet implemented
    // [InlineData("wildcard-q-star", "te?t*")]
    // [InlineData("wildcard-q", "te?t")]
    // // [InlineData("wildcard-q2", "te?t abc")]
    // [InlineData("no-wildcard-q", "\"te?t\"")]
    // [InlineData("wildcard-start", "*ext")]
    // [InlineData("wildcard", "te*t")]
    // [InlineData("wildcard", " te*t  ")]
    // [InlineData("no-wildcard", " \"te*t\"  ")] // * is not wildcard because inside quotes
    // //
    // // // fuzzy search FuzzyQuery
    // [InlineData("fuzzy", "roam~")]
    // [InlineData("fuzzy08", "roam~0.8")] // same as above due to removing 'MaxEdits' property in verify
    // [InlineData("fuzzy10", "roam~1.0")] // same as above due to removing 'MaxEdits' property in verify
    //
    // // //[InlineData("Proximity", "\"jakarta apache\"~10")] // Proximity Searches, PhraseQuery
    // // // [InlineData("boosting", "jakarta^4 apache")] // Proximity Searches, PhraseQuery
    // //
    // [InlineData("multi-001", "(+tag:github.com OR +tag:github)", false)] // or, two tags
    // [InlineData("multi-001", "+tag:github.com OR +tag:github", true)] // or, two tags
    // [InlineData("multi-001", "+tag:github.com +tag:github", false)] // or, two tags
    // [InlineData("multi-001", "+tag:github.com +tag:github", true)] // or, two tags
    // // [InlineData("multi-001", "(tag:github.com OR tag:github)")] // or, two tags
    // // [InlineData("multi-001", "(+tag:github.com OR +tag:github)")] // or, two tags
    // // [InlineData("multi-001.1", "+(tag:github.com OR tag:github)")] // or, two tags
    // // [InlineData("multi-001.2", "-(tag:github.com OR tag:github)")] // negative or, two tags
    // // [InlineData("multi-001.3", "(-tag:github.com AND -tag:github)")] // negative or, two tags
    // //
    // // [InlineData("multi-002", "is:pinned repom")]
    // // [InlineData("multi-002", "is:pinned AND repom")]
    // // [InlineData("multi-002", "is:pinned OR repom")] 
    //
    // public async Task ExperimentalTest(string outputName, string input, bool and = false)
    // {
    //     // arrange
    //     if (and)
    //     {
    //         // _queryParser.DefaultOperator = Operator.AND;
    //         _queryParser = new CustomMultiFieldQueryParser(LuceneNetVersion.VERSION, new[] { "free-text", }, _analyzer)
    //             {
    //                 DefaultOperator = Operator.AND,
    //                 AllowLeadingWildcard = true,
    //                 FuzzyMinSim = 1.0f, //FuzzyQuery 
    //                 PhraseSlop = 0, // disable Proximity
    //                 
    //             };
    //     }
    //     else
    //     {
    //         _queryParser = new CustomMultiFieldQueryParser(LuceneNetVersion.VERSION, new[] { "free-text", }, _analyzer)
    //             {
    //                 DefaultOperator = Operator.OR,
    //                 AllowLeadingWildcard = true,
    //                 FuzzyMinSim = 1.0f, //FuzzyQuery 
    //                 PhraseSlop = 0, // disable Proximity
    //             };
    //         // _queryParser.DefaultOperator = Operator.OR;
    //     }
    //     
    //
    //     // act
    //     Query result = _queryParser.Parse(input);
    //
    //     // assert
    //     await Verifier.Verify(new
    //                           {
    //                               Output = result.ToString(),
    //                               Lucene = result,
    //                               RepoM = MapQuery1(result, _queryParser.DefaultOperator == Operator.AND),
    //                           },
    //                       _settings)
    //                   .AddExtraSettings(x =>
    //                       {
    //                           x.TypeNameHandling = TypeNameHandling.All;
    //                       })
    //                   .UseTextForParameters(outputName);
    // }
    //
    // [Theory]
    // [InlineData("-tag:x")]
    // public async Task NegativeSingle(string input)
    // {
    //     // arrange
    //     _queryParser.DefaultOperator = Operator.OR;
    //
    //     // act
    //     Query result = _queryParser.Parse(input);
    //     var mapped = MapQuery1(result, _queryParser.DefaultOperator == Operator.AND);
    //     
    //     // assert
    //     var expected = new Not(new SimpleTerm("tag", "x")) as TermBase;
    //     await Verifier.Verify(mapped, _settings).AddExtraSettings(x =>
    //                       {
    //                           x.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
    //                           x.TypeNameHandling = TypeNameHandling.All;
    //                       })
    //                   .UseTextForParameters("xx");
    // }
    //
    // [Fact]
    // public async Task Tesdt1()
    // {
    //     // arrange
    //     var sut = new RepositoryIndex(new LuceneDirectoryInstance(new RamLuceneDirectoryFactory()));
    //     var item = new RepositorySearchModel(
    //         "c:/a/b/c",
    //         "RepoM",
    //         new List<string>()
    //             {
    //                 "repositories",
    //                 "work",
    //             });
    //     await sut.ReIndexMediaFileAsync(item).ConfigureAwait(false);
    //
    //     // act
    //     _ = sut.Search("tag:work project x", SearchOperator.Or, out var hits);
    //
    //     // assert
    //     hits.Should().Be(1);
    // }
    //
    // [Fact]
    // public async Task ExperimentalTests()
    // {
    //     // arrange
    //     // var analyzer = new WhitespaceAnalyzer(LuceneNetVersion.VERSION);
    //     // var analyzer = new KeywordAnalyzer();
    //
    //     // act
    //     Query result = _queryParser.Parse("tag:work test1 age:[41 TO 51] abc Core");
    //
    //     MyQueryBase x = MapQuery(result);
    //
    //     // assert
    //     await Verifier.Verify(new
    //                       {
    //                           Lucene = result,
    //                           RepoM = x,
    //                       },
    //                       _settings);
    // }

    

    /*
    private static TermBase MapQuery1(Query query, bool and)
    {
        if (query is Lucene.Net.Search.BooleanQuery bq)
        {
            // and, or
            TermBase[] items = bq.Clauses.Select(x =>
                {
                    if (x.Occur == Lucene.Net.Search.Occur.MUST_NOT)
                    {
                        return new Not(MapQuery1(x.Query, and));
                    }

                    return MapQuery1(x.Query, and);
                }).ToArray();


            if (items.Length == 1)
            {
                return items.Single();
            }

            return and ? new And(items) : new Or(items);
        }
        
        if (query is Lucene.Net.Search.TermQuery tq)
        {
            return new SimpleTerm(tq.Term.Field, tq.Term.Text);
        }

        if (query is Lucene.Net.Search.TermRangeQuery trq)
        {
            return new TermRange(
                trq.Field,
                Term.ToString(trq.LowerTerm),
                trq.IncludesLower,
                Term.ToString(trq.UpperTerm),
                trq.IncludesUpper);
        }
        
        if (query is Lucene.Net.Search.WildcardQuery wq)
        {
            return new WildCardTerm(wq.Term.Field, wq.Term.Text);
        }

        if (query is FuzzyQuery fq)
        {
            // do not support fuzzy query,
            return new SimpleTerm(fq.Term.Field, fq.Term.Text);
        }

        var fullName = query.GetType().FullName;
        throw new NotImplementedException(fullName);
    }
    */

    /*
    private static MyQueryBase MapQuery(Query query)
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

        if (query is Lucene.Net.Search.WildcardQuery wq)
        {
            return new MyWildcardQuery(wq);
        }

        if (query is FuzzyQuery fq)
        {
            // do not support fuzzy query,
            return new MyTermQuery(new MyTerm()
                {
                    Field = fq.Term.Field,
                    Text = fq.Term.Text,
                });
        }

        if (query is Lucene.Net.Search.PhraseQuery pq)
        {
            // do not support PhraseQuery,
            var fullName2 = query.GetType().FullName;
            throw new NotImplementedException(fullName2);
        }

        var fullName = query.GetType().FullName;
        throw new NotImplementedException(fullName);
    }
*/
}

/*
public sealed class MyTerm
{
    public string Field { get; init; }

    public string Text { get; init;  }
}
*/

/*
public class MyBooleanClause
{
    // LUCENENET specific - de-nested Occur from BooleanClause in order to prevent
    // a naming conflict with the Occur property

    public static string ToString(Occur occur)
    {
        return occur switch
            {
                Occur.MUST => "+",
                Occur.SHOULD => "",
                Occur.MUST_NOT => "-",
                _ => throw new ArgumentOutOfRangeException("Invalid Occur value")
            };
    }

    /// <summary>
    /// The query whose matching documents are combined by the boolean query.
    /// </summary>
    private MyQueryBase _query;

    private Occur _occur;

    /// <summary>
    /// Constructs a <see cref="BooleanClause"/>.
    /// </summary>
    public MyBooleanClause(MyQueryBase query, Occur occur)
    {
        _query = query;
        _occur = occur;
    }

    public virtual Occur Occur
    {
        get => _occur;
        set => _occur = value;
    }

    public virtual MyQueryBase Query
    {
        get => _query;
        set => _query = value;
    }
}
*/

/*
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
*/

/*
public abstract class MyQueryBase
{
}
*/



/*
public class MyWildcardQuery : MyQueryBase
{
    public MyWildcardQuery(WildcardQuery wildcardQuery)
    {
        Term = new MyTerm
            {
                Field = wildcardQuery.Term.Field,
                Text = wildcardQuery.Term.Text,
            };
        Field = wildcardQuery.Term.Field;
    }

    public MyTerm Term { get; init; }

    public string Field { get; init; }
}
*/

/*
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
*/

/*
public class MyTermQuery : MyQueryBase
{
    internal MyTermQuery(TermQuery termQuery):
        this(new MyTerm
            {
                Field = termQuery.Term.Field,
                Text = termQuery.Term.Text,
            })
    {
    }

    public MyTermQuery(MyTerm term)
    {
        Term = term;
    }

    public MyTerm Term { get; init; }
}
*/

/*public class MyBooleanQuery : MyQueryBase
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
                _ => throw new ArgumentOutOfRangeException(nameof(clauseOccur), clauseOccur, null),
            };
    }
}*/