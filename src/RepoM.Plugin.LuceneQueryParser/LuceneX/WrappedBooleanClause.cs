namespace RepoM.Plugin.LuceneQueryParser.LuceneX;

using Lucene.Net.Search;

public class WrappedBooleanClause : Lucene.Net.Search.BooleanClause
{
    private readonly BooleanClause _wrapped;

    public WrappedBooleanClause(Lucene.Net.Search.BooleanClause wrapped) :
        base(wrapped.Query, wrapped.Occur)
    {
        _wrapped = wrapped;
    }

    public override string ToString()
    {
        return Query.ToString();
    }
}