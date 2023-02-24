namespace RepoM.Core.Plugin.RepositoryFiltering.Clause;

public sealed class FalseQuery : IQuery
{
    private FalseQuery()
    {
    }

    public static FalseQuery Instance { get; } = new FalseQuery();
    
    public override string ToString()
    {
        return "False";
    }
}