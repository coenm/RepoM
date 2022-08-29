namespace RepoM.Api.Git;

public class DefaultRepositoryObserverFactory : IRepositoryObserverFactory
{
    public IRepositoryObserver Create()
    {
        return new DefaultRepositoryObserver();
    }
}