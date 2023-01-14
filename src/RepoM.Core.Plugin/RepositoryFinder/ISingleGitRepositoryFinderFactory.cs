namespace RepoM.Core.Plugin.RepositoryFinder;

public interface ISingleGitRepositoryFinderFactory
{
    string Name { get; }

    bool IsActive { get; }

    IGitRepositoryFinder Create();
}