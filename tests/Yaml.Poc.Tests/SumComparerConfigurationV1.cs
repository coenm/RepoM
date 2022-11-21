namespace Yaml.Poc.Tests;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering;

public class SumComparerConfigurationV1 : ICompareReposConfiguration
{
    public List<ICompareReposConfiguration> Comparers { get; set; }
}