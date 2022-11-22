namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Az;

public class AzRepositoryComparerFactory : IRepositoryComparerFactory<AlphabetComparerConfigurationV1>
{
    public IRepositoryComparer Create(AlphabetComparerConfigurationV1 configuration)
    {
        return new AzComparer();
    }
}