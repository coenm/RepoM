namespace RepoM.Api.IO;

using RepoM.Core.Plugin.RepositoryFinder;

public interface IGitRepositoryFinderFactory
{
    IGitRepositoryFinder Create();
}