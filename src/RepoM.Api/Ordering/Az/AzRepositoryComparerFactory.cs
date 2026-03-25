namespace RepoM.Api.Ordering.Az;

using RepoM.Core.Plugin.RepositoryOrdering;

public sealed class AzRepositoryComparerFactory : IRepositoryComparerFactory<AlphabetComparerConfigurationV1>
{
    public IRepositoryComparer Create(AlphabetComparerConfigurationV1 configuration)
    {
        return new AzComparer(configuration.Weight, configuration.Property);
    }
}