namespace RepoM.Plugin.LuceneQueryParser.Internal;

using System.Collections.Generic;
using System.Linq;

internal class SetBooleanClause : WrappedBooleanClause
{
    public SetBooleanClause(WrappedBooleanClause clause) : base(clause)
    {
        Add(clause);
    }

    public SetBooleanClause(params WrappedBooleanClause[] items) : base(items[0])
    {
        foreach (WrappedBooleanClause item in items)
        {
            Add(item);
        }
    }

    public BoolMode Mode { get; init; } = BoolMode.Nothing;

    public List<WrappedBooleanClause> Items { get; } = new List<WrappedBooleanClause>();

    private void Add(WrappedBooleanClause bc)
    {
        Items.Add(bc);
    }

    public enum BoolMode
    {
        /// <summary>
        /// And
        /// </summary>
        And,

        /// <summary>
        /// Or
        /// </summary>
        Or,

        /// <summary>
        /// Nothing
        /// </summary>
        Nothing,
    }

    public override string ToString()
    {
        if (Items.Count == 1)
        {
            return Items.Single().ToString();
        }

        var op = Mode == BoolMode.And ? "And" : "Or";
        return $"{op}( " + string.Join(", ", Items.Select(x => x.ToString())) + " )";
    }
}