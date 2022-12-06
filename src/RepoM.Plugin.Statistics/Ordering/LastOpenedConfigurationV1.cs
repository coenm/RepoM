namespace RepoM.Plugin.Statistics.Ordering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public sealed class LastOpenedConfigurationV1 : IRepositoriesComparerConfiguration
{
    public LastOpenedConfigurationV1()
    {
    }

    public int Weight { get; set; }
}