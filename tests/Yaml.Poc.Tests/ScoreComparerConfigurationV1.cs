namespace Yaml.Poc.Tests;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class ScoreComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public IRepositoryScorerConfiguration? ScoreProvider { get; set; }
}