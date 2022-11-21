namespace Yaml.Poc.Tests;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class AlphabetComparerConfigurationV1 : IRepositoriesCompareConfiguration
{
    public string? Property { get; set; }

    public int Weight { get; set; }
}