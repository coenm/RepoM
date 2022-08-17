namespace RepoM.Plugin.AzureDevOps;

using System;

internal class PullRequest
{
    public PullRequest(Guid repoId, string name, string url)
    {
        RepoId = repoId;
        Name = name;
        Url = url;
    }

    public Guid RepoId { get; }

    public string Name { get; }

    public string Url { get; }
}