namespace RepoM.Core.Plugin.RepositoryFinder;

public interface IPathSkipper
{
    bool ShouldSkip(string path);
}