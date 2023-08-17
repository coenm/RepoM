namespace RepoM.Plugin.AzureDevOps.Internal;

using System;

internal interface IAzureDevopsConfiguration
{
    string? AzureDevOpsPersonalAccessToken { get; }

    Uri? AzureDevOpsBaseUrl { get; }

    string? DefaultProjectId { get; }

    TimeSpan IntervalUpdateProjects { get; }

    TimeSpan IntervalUpdatePullRequests { get; }
}