namespace RepoM.Plugin.LuceneQueryParser.Plugin.Clause;

using System;
using LibGit2Sharp;
using static RepoM.Plugin.LuceneQueryParser.LuceneX.SetBooleanClause;
using System.Linq;

public class OrQuery : IQuery
{
    public IQuery[] Items { get; }

    public OrQuery(params IQuery[] items)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    public override string ToString()
    {
        if (Items.Length == 1)
        {
            return Items.Single().ToString();
        }

        return "Or( " + string.Join(", ", Items.Select(x => x.ToString())) + " )";
    }
}