namespace RepoM.Core.Plugin.RepositoryFiltering.Clause;

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