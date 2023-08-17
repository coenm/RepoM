namespace RepoM.Plugin.AzureDevOps.PersistentConfiguration;

using System;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
[Obsolete("Use Version V2. This version is supported until 2023-08-17 and will be removed after.")]
public class AzureDevopsConfigV1
{
    internal const int VERSION = 1;

    /// <summary>
    /// Personal access token (PAT) to access Azure Devops. The PAT should be granted access to `todo` rights.
    /// To create a PAT, goto `https://dev.azure.com/[my-organisation]/_usersSettings/tokens`.
    /// </summary>
    public string? PersonalAccessToken { get; init; }

    /// <summary>
    /// The base url of azure devops for your organisation (ie. `https://dev.azure.com/[my-organisation]/`).
    /// </summary>
    public string? BaseUrl { get; init; }
}