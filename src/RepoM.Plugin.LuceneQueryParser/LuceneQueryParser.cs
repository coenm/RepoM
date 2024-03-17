namespace RepoM.Plugin.LuceneQueryParser;

using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Plugin.LuceneQueryParser.Internal;

public class LuceneQueryParser : INamedQueryParser
{
    private readonly ILogger _logger;
    private const string KEY_FREE_TEXT = "ThisShouldBeAnUnguessableKEj";
    private readonly CustomMultiFieldQueryParser _queryParser;

    public LuceneQueryParser(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var analyzer = new WhitespaceAnalyzer(LuceneVersion.LUCENE_48);

        _queryParser = new CustomMultiFieldQueryParser(LuceneVersion.LUCENE_48, [KEY_FREE_TEXT,], analyzer)
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
            return ConvertSetQuery(result);
        }
        catch (ParseException e)
        {
            _logger.LogDebug(e, "Parse exception '{Text}' could not be parsed", text);
            throw;
        }
        catch (Exception e)
        {
            // Should not happen. Log just in case.
            _logger.LogError(e, "Unexpected Parse exception '{Text}' could not be parsed {Message}", text, e.Message);
            throw;
        }
    }

    private static IQuery ConvertSetQuery(SetQuery result)
    {
        return ConvertWrappedBooleanClause(result.SetBooleanClause);
    }

    private static IQuery ConvertWrappedBooleanClause(WrappedBooleanClause input)
    {
        if (input is NotBooleanClause nbc)
        {
            return new NotQuery(ConvertQueryToClause(nbc.Query));
        }

        if (input is SetBooleanClause setBooleanClause)
        {
            if (setBooleanClause.Items.Count == 1)
            {
                return ConvertWrappedBooleanClause(setBooleanClause.Items.Single());
            }

            IQuery[] array = setBooleanClause.Items.Select(ConvertWrappedBooleanClause).ToArray();

            return setBooleanClause.Mode == SetBooleanClause.BoolMode.And
                ? new AndQuery(array)
                : new OrQuery(array);
        }

        return ConvertQueryToClause(input.Query);
    }

    private static IQuery ConvertQueryToClause(Query query)
    {
        switch (query)
        {
            case TermQuery tq:
                return ConvertTermQuery(tq);
            case TermRangeQuery trq:
                return ConvertTermRangeQuery(trq);
            case WildcardQuery wq:
                return ConvertWildcardQuery(wq);
            case FuzzyQuery fq:
                return ConvertFuzzyQuery(fq);
            case SetQuery cq:
                return ConvertSetQuery(cq);
            case PrefixQuery pq:
                return ConvertPrefixQuery(pq);
            case PhraseQuery phraseQuery:
                return ConvertPhraseQuery(phraseQuery);
            default:
                {
                    var fullName = query.GetType().FullName;
                    throw new NotImplementedException(fullName);
                }
        }
    }

    private static IQuery ConvertPhraseQuery(PhraseQuery phraseQuery)
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

    private static IQuery ConvertPrefixQuery(PrefixQuery pq)
    {
        return new StartsWithTerm(pq.Field, pq.Prefix.Text);
    }

    private static IQuery ConvertFuzzyQuery(FuzzyQuery fq)
    {
        // do not support fuzzy query,
        if (KEY_FREE_TEXT.Equals(fq.Term.Field, StringComparison.CurrentCulture))
        {
            return new FreeText(fq.Term.Text);
        }

        return new SimpleTerm(fq.Term.Field, fq.Term.Text);
    }

    private static WildCardTerm ConvertWildcardQuery(WildcardQuery wq)
    {
        return new WildCardTerm(wq.Term.Field, wq.Term.Text);
    }

    private static TermRange ConvertTermRangeQuery(TermRangeQuery trq)
    {
        return new TermRange(
            trq.Field,
            trq.LowerTerm == null ? null : Term.ToString(trq.LowerTerm),
            trq.IncludesLower,
            trq.UpperTerm == null ? null : Term.ToString(trq.UpperTerm),
            trq.IncludesUpper);
    }

    private static IQuery ConvertTermQuery(TermQuery tq)
    {
        if (KEY_FREE_TEXT.Equals(tq.Term.Field, StringComparison.CurrentCulture))
        {
            return new FreeText(tq.Term.Text);
        }

        return new SimpleTerm(tq.Term.Field, tq.Term.Text);
    }
}