namespace RepoM.Plugin.AzureDevOps.Internal;

using System;

internal class PullRequest
{
    public PullRequest(Guid repositoryId, string name, string url)
    {
        RepositoryId = repositoryId;
        Name         = name;
        Url          = url;
    }

    public Guid RepositoryId { get; }

    public string Name { get; }

    public string Url { get; }
}