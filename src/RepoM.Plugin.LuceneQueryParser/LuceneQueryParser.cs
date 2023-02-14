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
using RepoM.Plugin.LuceneQueryParser.Internal;

public class LuceneQueryParser : INamedQueryParser
{
    private const string KEY_FREE_TEXT = "ThisShouldBeAnUnguessableKEj";
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

    public string Name => "Lucene";

    public IQuery Parse(string text)
    {
        try
        {
            var result = (SetQuery)_queryParser.Parse(text);
            return MapQuery(result);
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

    private IQuery MapQuery(SetQuery result)
    {
        return ConvertWrappedBooleanClause(result.SetBooleanClause);
    }

    private IQuery ConvertWrappedBooleanClause(WrappedBooleanClause input)
    {
        if (input is NotBooleanClause nbc)
        {
            return new NotQuery(ConvertQueryToClause(nbc.Query));
        }

        if (input is SetBooleanClause x)
        {
            if (x.Items.Count == 1)
            {
                return ConvertWrappedBooleanClause(x.Items.Single());
            }

            IQuery[] array = x.Items.Select(ConvertWrappedBooleanClause).ToArray();

            return x.Mode == SetBooleanClause.BoolMode.AND
                ? new AndQuery(array)
                : new OrQuery(array);
        }

        return ConvertQueryToClause(input.Query);
    }

    private IQuery ConvertQueryToClause(Query query)
    {
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
            if (KEY_FREE_TEXT.Equals(fq.Term.Field, StringComparison.CurrentCulture))
            {
                return new FreeText(fq.Term.Text);
            }

            return new SimpleTerm(fq.Term.Field, fq.Term.Text);
        }

        if (query is SetQuery cq)
        {
            return MapQuery(cq);
        }
        
        if (query is PrefixQuery pq)
        {
            return new StartsWithTerm(pq.Field, pq.Prefix.Text);
        }

        if (query is PhraseQuery phraseQuery)
        {
            var queryTerms = new HashSet<Term>();
            phraseQuery.ExtractTerms(queryTerms);

            if (queryTerms.Count < 1)
            {
                throw new NotSupportedException();
            }

            // assume all same field
            var field = queryTerms.First().Field;
            var s = queryTerms.Aggregate(string.Empty, (current, qt) => current + " " + qt.Text).Trim();

            if (KEY_FREE_TEXT.Equals(field, StringComparison.CurrentCulture))
            {
                return new FreeText(s);
            }
                
            return new SimpleTerm(field, s);

            // not supported
        }

        var fullName = query.GetType().FullName;
        throw new NotImplementedException(fullName);
    }
}