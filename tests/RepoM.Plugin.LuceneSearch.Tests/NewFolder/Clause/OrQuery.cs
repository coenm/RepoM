namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause;
public class OrQuery : IQuery
{
    public IQuery[] Items { get; }

    public OrQuery(params IQuery[] items)
    {
        Items = items;
    }
}