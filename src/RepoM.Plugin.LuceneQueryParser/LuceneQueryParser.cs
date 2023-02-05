namespace RepoM.Plugin.LuceneQueryParser;

using System;
using System.Linq;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Plugin.LuceneQueryParser.LuceneX;

public class LuceneQueryParser1 : INamedQueryParser
{
    private readonly WhitespaceAnalyzer _analyzer;
    private readonly CustomMultiFieldQueryParser _queryParser;

    public LuceneQueryParser1()
    {
        _analyzer = new WhitespaceAnalyzer(LuceneVersion.LUCENE_48);

        _queryParser = new CustomMultiFieldQueryParser(LuceneVersion.LUCENE_48, new[] { "free-text", }, _analyzer)
        {
            DefaultOperator = Operator.AND,
            AllowLeadingWildcard = true,
            FuzzyMinSim = 1.0f, //FuzzyQuery 
            PhraseSlop = 0, // disable Proximity
        };
    }

    public string Name { get; } = "Lucene";

    public IQuery Parse(string text)
    {
        var result = (SetQuery)_queryParser.Parse(text);
        return MapQuery2(result, _queryParser.DefaultOperator == Operator.AND);
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
        if (query is TermQuery tq)
        {
            return new SimpleTerm(tq.Term.Field, tq.Term.Text);
        }

        if (query is TermRangeQuery trq)
        {
            return new TermRange(
                trq.Field,
                Term.ToString(trq.LowerTerm),
                trq.IncludesLower,
                Term.ToString(trq.UpperTerm),
                trq.IncludesUpper);
        }

        if (query is WildcardQuery wq)
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
}