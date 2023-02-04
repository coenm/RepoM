namespace RepoM.Plugin.LuceneQueryParser.Plugin.Clause;

using System.Linq;
using System;

public class AndQuery : IQuery
{
    public IQuery[] Items { get; }

    public AndQuery(params IQuery[] items)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    public override string ToString()
    {
        if (Items.Length == 1)
        {
            return Items.Single().ToString();
        }

        return "And( " + string.Join(", ", Items.Select(x => x.ToString())) + " )";
    }
}