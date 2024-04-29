namespace RepoM.Core.Plugin.RepositoryFinder;

public interface ISingleGitRepositoryFinderFactory
{
    IGitRepositoryFinder Create();
}