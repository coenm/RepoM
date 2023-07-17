namespace RepoM.Plugin.AzureDevOps.Internal;

using System;

internal class AzureDevopsConfiguration : IAzureDevopsConfiguration
{
    public AzureDevopsConfiguration(string? url, string? pat)
    {
        AzureDevOpsPersonalAccessToken = pat;

        try
        {
            AzureDevOpsBaseUrl = url != null ? new Uri(url) : null!;
        }
        catch (Exception e)
        {
            AzureDevOpsBaseUrl = null;
        }
    }

    public string? AzureDevOpsPersonalAccessToken { get; }

    public Uri? AzureDevOpsBaseUrl { get; }
}