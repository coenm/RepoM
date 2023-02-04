namespace RepoM.Plugin.LuceneQueryParser.LuceneX;

using Lucene.Net.Search;

public class SetQuery : Query
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