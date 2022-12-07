namespace RepoM.Plugin.Statistics.Ordering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public sealed class LastOpenedConfigurationV1 : IRepositoriesComparerConfiguration
{
    public int Weight { get; set; }
}