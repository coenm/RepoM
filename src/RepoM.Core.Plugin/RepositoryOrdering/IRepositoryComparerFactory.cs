namespace RepoM.Core.Plugin.RepositoryOrdering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public interface IRepositoryComparerFactory<TConfig>
    where TConfig : IRepositoriesComparerConfiguration
{
    IRepositoryComparer Create(TConfig configuration);
}