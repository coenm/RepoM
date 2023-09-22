namespace RepoM.Plugin.LuceneQueryParser.Internal;

using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;

internal class CustomMultiFieldQueryParser : MultiFieldQueryParser
{
    private static readonly BooleanClause _dummy = new(new BooleanQuery(), Occur.MUST_NOT);

    public CustomMultiFieldQueryParser(LuceneVersion matchVersion, string[] fields, Analyzer analyzer)
        : base(matchVersion, fields, analyzer)
    {
    }

    protected override Query GetBooleanQuery(IList<BooleanClause> clauses, bool disableCoord)
    {
        if (clauses.Count == 1)
        {
            if (clauses[0] is BooleanClause booleanClause)
            {
                WrappedBooleanClause wrappedBooleanClause = booleanClause.IsProhibited
                    ? new NotBooleanClause(booleanClause)
                    : new WrappedBooleanClause(booleanClause);
                clauses[0] = new SetBooleanClause(wrappedBooleanClause) { Mode = SetBooleanClause.BoolMode.And, };
                clauses.Add(_dummy); 
            }
        }

        if (clauses[0] is not SetBooleanClause setBooleanClause)
        {
            return base.GetBooleanQuery(clauses, disableCoord);
        }

        return new SetQuery(setBooleanClause);
    }
    
    protected override void AddClause(IList<BooleanClause> clauses, int conj, int mods, Query q)
    {
        // We might have been passed a null query; the term might have been
        // filtered away by the analyzer.
        if (q is null)
        {
            return;
        }

        Occur occur = mods == MOD_NOT ? Occur.MUST_NOT : Occur.MUST;

        var clause = (WrappedBooleanClause)NewBooleanClause(q, occur);

        if (!clauses.Any())
        {
            // default AND
            clauses.Add(new SetBooleanClause(clause) { Mode = SetBooleanClause.BoolMode.And, });
            clauses.Add(_dummy); // dummy
            return;
        }

        if (conj is CONJ_NONE or CONJ_AND)
        {
            // and
            var currentClause = (SetBooleanClause)clauses[0];

            if (currentClause.Mode == SetBooleanClause.BoolMode.And)
            {
                currentClause.Items.Add(new SetBooleanClause(clause));
            }
            else
            {
                // or (x, y, z)
                // pick last item to make it
                // or (x, y, (and (z, zz))
                WrappedBooleanClause lastItem = currentClause.Items[^1];
                ((SetBooleanClause)lastItem).Items.Add(new SetBooleanClause(clause));
            }
        }
        else
        {
            // or

            var currentClause = (SetBooleanClause)clauses[0];

            if (currentClause.Mode == SetBooleanClause.BoolMode.Or)
            {
                currentClause.Items.Add(new SetBooleanClause(clause));
            }
            else
            {
                // start re-ordering.
                var second = new SetBooleanClause(clause) { Mode = SetBooleanClause.BoolMode.And, };
                var combination = new SetBooleanClause(currentClause, second) { Mode = SetBooleanClause.BoolMode.Or, };
                clauses[0] = combination;
            }
        }
    }

    protected override BooleanClause NewBooleanClause(Query q, Occur occur)
    {
        if (occur != Occur.MUST_NOT)
        {
            occur = Occur.MUST;
        }

        BooleanClause result = base.NewBooleanClause(q, occur);

        return occur == Occur.MUST_NOT
            ? new NotBooleanClause(result)
            : new WrappedBooleanClause(result);
    }
}