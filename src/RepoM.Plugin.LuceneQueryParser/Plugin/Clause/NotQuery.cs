namespace RepoM.Plugin.LuceneQueryParser.Plugin.Clause;
using Lucene.Net.Search;

public class NotQuery : IQuery
{
    public IQuery Item { get; }

    public NotQuery(IQuery item)
    {
        Item = item;
    }

    public override string ToString()
    {
        return $"Not({Item})";
    }
}