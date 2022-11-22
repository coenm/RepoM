namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Composition;

using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class CompositionComparerConfigurationV1 : IRepositoriesComparerConfiguration
{
    public List<IRepositoriesComparerConfiguration> Comparers { get; set; } = new();
}