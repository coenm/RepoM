namespace RepoM.Plugin.LuceneQueryParser;

using System;
using System.Collections.Generic;
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

public class LuceneQueryParser : INamedQueryParser
{
    private const string KEY_FREE_TEXT = "ThisIsAnUnguessableKEj";
    private readonly CustomMultiFieldQueryParser _queryParser;

    public LuceneQueryParser()
    {
        var analyzer = new WhitespaceAnalyzer(LuceneVersion.LUCENE_48);

        
        _queryParser = new CustomMultiFieldQueryParser(LuceneVersion.LUCENE_48, new[] { KEY_FREE_TEXT, }, analyzer)
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
        try
        {
            var result = (SetQuery)_queryParser.Parse(text);
            return MapQuery2(result, _queryParser.DefaultOperator == Operator.AND);
        }
        catch (ParseException e)
        {
            Console.WriteLine(e);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
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
         if (query is Lucene.Net.Search.BooleanQuery bq)
         {
            // // and, or
            // TermBase[] items = bq.Clauses.Select(x =>
            //     {
            //         if (x.Occur == Lucene.Net.Search.Occur.MUST_NOT)
            //         {
            //             return new Not(MapQuery1(x.Query, and));
            //         }
            //
            //         return MapQuery1(x.Query, and);
            //     }).ToArray();
            //
            //
            // if (bq.Clauses.Count == 1)
            // {
            //     return bq.Clauses.Single();
            // }
            //
            // return and ? new And(items) : new Or(items);
            // return new MyBooleanQuery(bq, MapQuery);
        }

        if (query is TermQuery tq)
        {
            if (KEY_FREE_TEXT.Equals(tq.Term.Field, StringComparison.CurrentCulture))
            {
                return new FreeText(tq.Term.Text);
            }

            return new SimpleTerm(tq.Term.Field, tq.Term.Text);
        }

        if (query is TermRangeQuery trq)
        {
            return new TermRange(
                trq.Field,
                trq.LowerTerm == null ? null : Term.ToString(trq.LowerTerm),
                trq.IncludesLower,
                trq.UpperTerm == null ? null : Term.ToString(trq.UpperTerm),
                trq.IncludesUpper);
        }

        if (query is WildcardQuery wq)
        {
            return new WildCardTerm(wq.Term.Field, wq.Term.Text);
        }
        
        if (query is FuzzyQuery fq)
        {
            // do not support fuzzy query,
            return new SimpleTerm(fq.Term.Field, fq.Term.Text);
        }

        if (query is SetQuery cq)
        {
            return MapQuery2(cq, true);
        }
        
        if (query is PrefixQuery pq)
        {
            return new StartsWithTerm(pq.Field, pq.Prefix.Text);
        }

        if (query is PhraseQuery phraseQuery)
        {
            var queryTerms = new HashSet<Term>();
            phraseQuery.ExtractTerms(queryTerms);

            if (queryTerms.Count >= 1)
            {
                // assume all same field
                var field = queryTerms.First().Field;
                var s = string.Empty;
                foreach (var qt in queryTerms)
                {
                    s += " " + qt.Text;
                }

                s = s.Trim();

                if (KEY_FREE_TEXT.Equals(field, StringComparison.CurrentCulture))
                {
                    return new FreeText(s);
                }
                
                return new SimpleTerm(field, s);
            }
        
            // not supported
            throw new NotSupportedException();
        }

        var fullName = query.GetType().FullName;
        throw new NotImplementedException(fullName);
    }
}