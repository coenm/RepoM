namespace RepoM.Plugin.AzureDevOps.Internal;

using System;

internal class AzureDevopsConfiguration : IAzureDevopsConfiguration
{
    public AzureDevopsConfiguration(string? url, string? pat, string? defaultProjectId, TimeSpan intervalUpdateProjects, TimeSpan intervalUpdatePullRequests)
    {
        AzureDevOpsPersonalAccessToken = pat;
        DefaultProjectId = defaultProjectId;
        IntervalUpdateProjects = intervalUpdateProjects;
        IntervalUpdatePullRequests = intervalUpdatePullRequests;

        try
        {
            AzureDevOpsBaseUrl = url != null ? new Uri(url) : null!;
        }
        catch (Exception)
        {
            AzureDevOpsBaseUrl = null;
        }
    }

    public string? AzureDevOpsPersonalAccessToken { get; }

    public Uri? AzureDevOpsBaseUrl { get; }

    public string? DefaultProjectId { get; }

    public TimeSpan IntervalUpdateProjects { get; }

    public TimeSpan IntervalUpdatePullRequests { get; }

}