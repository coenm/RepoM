namespace RepoM.Plugin.LuceneQueryParser.Internal;

using Lucene.Net.Search;

internal class NotBooleanClause : WrappedBooleanClause
{
    public NotBooleanClause(BooleanClause clause)
        : base(clause)
    {
    }

    public override string ToString()
    {
        return $"Not({Query})";
    }
}