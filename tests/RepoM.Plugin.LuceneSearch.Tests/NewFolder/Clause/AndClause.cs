namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause;
public class AndClause : IClause
{
    public IClause[] Items { get; }

    public AndClause(params IClause[] items)
    {
        Items = items;
    }
}