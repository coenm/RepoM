namespace RepoM.Plugin.LuceneQueryParser.Internal;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;

internal class CustomMultiFieldQueryParser : MultiFieldQueryParser
{
    private static readonly BooleanClause _dummy = new(new BooleanQuery(), Occur.MUST_NOT);

    public CustomMultiFieldQueryParser(LuceneVersion matchVersion, string[] fields, Analyzer analyzer, IDictionary<string, float> boosts)
        : base(matchVersion, fields, analyzer, boosts)
    {
    }

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
        bool required, prohibited;

        // We might have been passed a null query; the term might have been
        // filtered away by the analyzer.
        if (q is null)
        {
            return;
        }

        if (DefaultOperator == OR_OPERATOR)
        {
            // We set REQUIRED if we're introduced by AND or +; PROHIBITED if
            // introduced by NOT or -; make sure not to set both.
            prohibited = mods == MOD_NOT;
            required = mods == MOD_REQ;
            if (conj == CONJ_AND && !prohibited)
            {
                required = true;
            }
        }
        else
        {
            // We set PROHIBITED if we're introduced by NOT or -; We set REQUIRED
            // if not PROHIBITED and not introduced by OR
            prohibited = mods == MOD_NOT;
            required = !prohibited && conj != CONJ_OR;
        }

        WrappedBooleanClause clause;
        if (required && !prohibited)
        {
            clause = (WrappedBooleanClause)NewBooleanClause(q, Occur.MUST);
        }
        else if (!required && !prohibited)
        {
            clause = (WrappedBooleanClause)NewBooleanClause(q, Occur.MUST);
        }
        else if (!required && prohibited)
        {
            clause = (WrappedBooleanClause)NewBooleanClause(q, Occur.MUST_NOT);
        }
        else
        {
            throw new Exception();
        }

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
                Debug.Assert(((SetBooleanClause)lastItem).Mode == SetBooleanClause.BoolMode.And);
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

        if (occur == Occur.MUST_NOT)
        {
            return new NotBooleanClause(result);
        }

        return new WrappedBooleanClause(result);
    }
}