namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.LuceneX;

using System.Collections.Generic;
using System.Linq;

public class SetBooleanClause : WrappedBooleanClause
{
    public SetBooleanClause(WrappedBooleanClause first) : base(first)
    {
        Add(first);
    }

    public SetBooleanClause(WrappedBooleanClause[] items) : base(items.First())
    {
        foreach (WrappedBooleanClause item in items)
        {
            Add(item);
        }
    }

    public BoolMode Mode { get; set; } = BoolMode.NOTHING;

    public List<WrappedBooleanClause> Items { get; } = new List<WrappedBooleanClause>();

    private void Add(WrappedBooleanClause bc)
    {
        Items.Add(bc);
    }

    public enum BoolMode
    {
        AND,
        OR,
        NOTHING,
    }

    public override string ToString()
    {
        if (Items.Count == 1)
        {
            return Items.Single().ToString();
        }

        var op = Mode == BoolMode.AND ? "And" : "Or";
        return $"{op}( " + string.Join(", ", Items.Select(x => x.ToString())) + " )";

    }
}