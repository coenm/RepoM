namespace RepoM.Core.Plugin.RepositoryOrdering;

public interface IRepositoryComparerFactory
{
    IRepositoryComparer Create();
}