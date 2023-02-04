namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause;
public class NotClause : IClause
{
    public IClause Item { get; }

    public NotClause(IClause item)
    {
        Item = item;
    }
}