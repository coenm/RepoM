namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause;
public class NotQuery : IQuery
{
    public IQuery Item { get; }

    public NotQuery(IQuery item)
    {
        Item = item;
    }
}