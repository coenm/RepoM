namespace RepoM.Core.Plugin.RepositoryFiltering.Clause;

public sealed class FalseQuery : IQuery
{
    public override string ToString()
    {
        return "False";
    }
}