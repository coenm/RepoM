namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.Clause;
public class AndQuery : IQuery
{
    public IQuery[] Items { get; }

    public AndQuery(params IQuery[] items)
    {
        Items = items;
    }
}