namespace RepoM.Plugin.AzureDevOps.PersistentConfiguration;

using System;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 2)</summary>
public class AzureDevopsConfigV2
{
    internal const int VERSION = 2;

    /// <summary>
    /// Personal access token (PAT) to access Azure Devops. The PAT should be granted access to `todo` rights.
    /// To create a PAT, goto `https://dev.azure.com/[my-organisation]/_usersSettings/tokens`.
    /// </summary>
    public string? PersonalAccessToken { get; init; }

    /// <summary>
    /// The base url of azure devops for your organisation (ie. `https://dev.azure.com/[my-organisation]/`).
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// Default project id to use when no project id provided in the repository action. Should be a GUID.
    /// </summary>
    public string? DefaultProjectId { get; init; }

    /// <summary>
    /// Interval RepoM should update the list of open pull requests from Azure DevOps. Defaults to `4` minutes (ie. `00:04:00`).
    /// </summary>
    public TimeSpan? IntervalUpdatePullRequests { get; init; }

    /// <summary>
    /// Interval RepoM should update the list of projects from Azure DevOps. Defaults to `10` minutes (ie. `00:10:00`).
    /// </summary>
    public TimeSpan? IntervalUpdateProjects { get; init; }
}