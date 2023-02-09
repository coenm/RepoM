namespace RepoM.Plugin.LuceneQueryParser.LuceneX;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Sprache;

public class CustomMultiFieldQueryParser : MultiFieldQueryParser
{
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
            if (clauses[0] is BooleanClause bc)
            {
                WrappedBooleanClause bc1;
                if (bc.IsProhibited)
                {
                    bc1 = new NotBooleanClause(bc);
                }
                else
                {
                    bc1 = new WrappedBooleanClause(bc);
                }
                clauses[0] = (new SetBooleanClause(bc1) { Mode = SetBooleanClause.BoolMode.AND, });
                clauses.Add(dummy); // dummy
            }
        }

        if (clauses[0] is not SetBooleanClause x)
        {
            return base.GetBooleanQuery(clauses, disableCoord);
            // throw new Exception();
        }

        return new SetQuery(x);
        //
        //     var l = new List<BooleanClause>();
        //     
        //     BooleanQuery query = NewBooleanQuery(disableCoord);
        //     foreach (BooleanClause clause in clauses)
        //     {
        // // protected const int CONJ_NONE = 0;
        // // protected const int CONJ_AND = 1;
        // // protected const int CONJ_OR = 2;
        //         var conj = 0;
        //         var mods = clause.IsProhibited ? MOD_NOT : MOD_REQ;
        //         AddClause(l, conj, mods, clause.Query);
        //         // query.Add(clause);
        //     }
        //
        //     return GetBooleanQuery(l);
        //
        //     // if (clauses.Count == 0)
        //     // {
        //     //     return null; // all clause words were filtered away by the analyzer.
        //     // }
        //     //
        //     // if (clauses.Count == 1)
        //     // {
        //     //     if (clauses[0] is SetBooleanClause x)
        //     //     {
        //     //         return new SetQuery(x);
        //     //     }
        //     // }
        //     //
        //     // throw new Exception();
    }

    private static BooleanClause dummy = new(new BooleanQuery(), Occur.MUST_NOT);

    //
    // protected override Query GetBooleanQuery(IList<BooleanClause> clauses)
    // {
    //     return GetBooleanQuery(clauses, false);
    //     return base.GetBooleanQuery(clauses);
    // }
    
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
            prohibited = mods == MOD_NOT; // verboden
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
            clauses.Add(new SetBooleanClause(clause) { Mode = SetBooleanClause.BoolMode.AND, });
            clauses.Add(dummy); // dummy
            return;
        }

        if (conj is CONJ_NONE or CONJ_AND)
        {
            // and
            var currentClause = (SetBooleanClause)clauses.First();

            if (currentClause.Mode == SetBooleanClause.BoolMode.AND)
            {
                currentClause.Items.Add(new SetBooleanClause(clause));
            }
            else
            {
                // or (x, y, z)
                // pick last item to make it
                // or (x, y, (and (z, zz))
                WrappedBooleanClause lastItem = currentClause.Items.Last();
                Debug.Assert(((SetBooleanClause)lastItem).Mode == SetBooleanClause.BoolMode.AND);
                ((SetBooleanClause)lastItem).Items.Add(new SetBooleanClause(clause));
            }
        }
        else
        {
            // or

            var currentClause = (SetBooleanClause)clauses.First();

            if (currentClause.Mode == SetBooleanClause.BoolMode.OR)
            {
                currentClause.Items.Add(new SetBooleanClause(clause));
            }
            else
            {
                // start re-ordering.
                var second = new SetBooleanClause(clause) { Mode = SetBooleanClause.BoolMode.AND, };
                var combination = new SetBooleanClause(new[] { currentClause, second, }) { Mode = SetBooleanClause.BoolMode.OR, };
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