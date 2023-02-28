namespace RepoM.Plugin.LuceneQueryParser.Internal;

internal class WrappedBooleanClause : Lucene.Net.Search.BooleanClause
{
    public WrappedBooleanClause(Lucene.Net.Search.BooleanClause wrapped) :
        base(wrapped.Query, wrapped.Occur)
    {
    }

    public override string ToString()
    {
        return Query.ToString();
    }
}