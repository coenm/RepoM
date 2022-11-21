namespace Yaml.Poc.Tests;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class SumComparerConfigurationV1 : IRepositoriesCompareConfiguration
{
    public List<IRepositoriesCompareConfiguration> Comparers { get; set; } = new();
}