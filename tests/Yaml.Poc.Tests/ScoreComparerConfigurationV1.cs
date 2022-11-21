namespace Yaml.Poc.Tests;

using RepoM.Core.Plugin.RepositoryOrdering;

public class ScoreComparerConfigurationV1 : ICompareReposConfiguration
{
    public IRepoScorerConfiguration? ScoreProvider { get; set; }
}