namespace RepoM.Api.Git;

public interface IRepositoryObserverFactory
{
    IRepositoryObserver Create();
}