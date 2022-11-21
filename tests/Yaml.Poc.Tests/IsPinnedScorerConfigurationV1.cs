namespace Yaml.Poc.Tests;

using RepoM.Core.Plugin.RepositoryOrdering;

public class IsPinnedScorerConfigurationV1 : IRepoScorerConfiguration
{
    public int Weight { get; set; }
}