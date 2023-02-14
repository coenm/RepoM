namespace RepoM.Core.Plugin.RepositoryFiltering.Clause;

using System;
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
        if (Items.Length == 0)
        {
            return string.Empty;
        }

        if (Items.Length == 1)
        {
            return Items.Single().ToString() ?? string.Empty;
        }

        return "Or( " + string.Join(", ", Items.Select(x => x.ToString())) + " )";
    }
}