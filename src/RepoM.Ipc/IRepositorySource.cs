namespace RepoM.Ipc;

public interface IRepositorySource
{
    Repository[] GetMatchingRepositories(string repositoryNamePattern);
}