namespace RepoM.Plugin.AzureDevOps.Internal;

using System;

internal class PullRequest(Guid repositoryId, string name, string url)
{
    public Guid RepositoryId { get; } = repositoryId;

    public string Name { get; } = name;

    public string Url { get; } = url;
}