namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause;
public class OrClause : IClause
{
    public IClause[] Items { get; }

    public OrClause(params IClause[] items)
    {
        Items = items;
    }
}