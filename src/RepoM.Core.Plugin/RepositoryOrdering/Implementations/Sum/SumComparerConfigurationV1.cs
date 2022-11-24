namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Sum;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class SumComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public List<IRepositoriesComparerConfiguration> Comparers { get; set; } = new();
}