namespace RepoM.Core.Plugin.RepositoryFiltering.Clause;

public sealed class TrueQuery : IQuery
{
    private TrueQuery()
    {
    }

    public static TrueQuery Instance { get; } = new TrueQuery();

    public override string ToString()
    {
        return "True";
    }
}