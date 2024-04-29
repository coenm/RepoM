namespace RepoM.Api.IO;

using System;
using RepoM.Core.Plugin.RepositoryFinder;

public class GitRepositoryFinderFactory : IGitRepositoryFinderFactory
{
    private readonly ISingleGitRepositoryFinderFactory _factory;

    public GitRepositoryFinderFactory(ISingleGitRepositoryFinderFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IGitRepositoryFinder Create()
    {
        return _factory.Create();
    }
}