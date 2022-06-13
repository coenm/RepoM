namespace RepoM.Api.Common.Git;

using RepoM.Api.Git;

public class DefaultRepositoryObserverFactory : IRepositoryObserverFactory
{
    public IRepositoryObserver Create()
    {
        return new DefaultRepositoryObserver();
    }
}