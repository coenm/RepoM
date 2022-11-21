namespace Yaml.Poc.Tests;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class ScoreComparerConfigurationV1 : IRepositoriesCompareConfiguration
{
    public IRepositoryScorerConfiguration? ScoreProvider { get; set; }
}