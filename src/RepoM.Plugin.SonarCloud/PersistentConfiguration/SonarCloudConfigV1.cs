namespace RepoM.Plugin.SonarCloud.PersistentConfiguration;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
public class SonarCloudConfigV1
{
    /// <summary>
    /// Personal Access Token to access SonarCloud.
    /// </summary>
    public string? PersonalAccessToken { get; init; }

    /// <summary>
    /// SonarCloud url. Most likely `https//sonarcloud.io`.
    /// </summary>
    public string? BaseUrl { get; init; }
}