namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class ScoreComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public IRepositoryScorerConfiguration? ScoreProvider { get; set; }
}