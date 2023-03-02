namespace RepoM.Core.Plugin.RepositoryFiltering.Clause;

using System;
using System.Linq;

public class AndQuery : IQuery
{
    public IQuery[] Items { get; }

    public AndQuery(params IQuery[] items)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    public override string ToString()
    {
        if (Items.Length == 0)
        {
            return string.Empty;
        }
        
        if (Items.Length == 1)
        {
            return Items.Single().ToString() ?? string.Empty;
        }

        return "And(" + string.Join(", ", Items.Select(x => x.ToString())) + ")";
    }
}