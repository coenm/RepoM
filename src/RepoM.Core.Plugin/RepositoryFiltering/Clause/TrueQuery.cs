namespace RepoM.Core.Plugin.RepositoryFiltering.Clause;

public sealed class TrueQuery : IQuery
{
    public override string ToString()
    {
        return "True";
    }
}