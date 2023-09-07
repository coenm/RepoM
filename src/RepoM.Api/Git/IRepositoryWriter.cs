namespace RepoM.Api.Git;

using RepoM.Core.Plugin.Repository;

public interface IRepositoryWriter
{
    bool Checkout(IRepository repository, string branchName);

    void Fetch(IRepository repository);

    void Pull(IRepository repository);

    void Push(IRepository repository);
}