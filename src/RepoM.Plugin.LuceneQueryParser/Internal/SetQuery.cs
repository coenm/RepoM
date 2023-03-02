namespace RepoM.Plugin.LuceneQueryParser.Internal;

using Lucene.Net.Search;

internal class SetQuery : Query
{
    public SetBooleanClause SetBooleanClause { get; }

    public SetQuery(SetBooleanClause setBooleanClause)
    {
        SetBooleanClause = setBooleanClause;
    }

    public override string ToString(string field)
    {
        return SetBooleanClause.ToString();
    }
}