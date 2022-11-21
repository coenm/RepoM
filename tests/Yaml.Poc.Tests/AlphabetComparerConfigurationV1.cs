namespace Yaml.Poc.Tests;

using RepoM.Core.Plugin.RepositoryOrdering;

public class AlphabetComparerConfigurationV1 : ICompareReposConfiguration
{
    public string? Property { get; set; }

    public int Weight { get; set; }
}