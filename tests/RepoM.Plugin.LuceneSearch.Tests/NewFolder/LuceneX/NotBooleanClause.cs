namespace RepoM.Plugin.LuceneSearch.Tests.NewFolder.LuceneX;

using Lucene.Net.Search;

public class NotBooleanClause : WrappedBooleanClause
{
    public NotBooleanClause(BooleanClause clause)
        : base(clause)
    {
    }

    public override string ToString()
    {
        return $"Not({Query.ToString()})";
    }
}