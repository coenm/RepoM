namespace RepoM.Plugin.AzureDevOps.PersistentConfiguration;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
public class AzureDevopsConfigV1
{
    /// <summary>
    /// Personal access token (PAT) to access Azure Devops. The PAT should be granted access to `todo` rights
    /// </summary>
    public string? PersonalAccessToken { get; init; }

    /// <summary>
    /// The base url of azure devops for your organisation (ie. `https://dev.azure.com/my-organisation/`).
    /// </summary>
    public string? BaseUrl { get; init; }
}