namespace RepoM.Plugin.SonarCloud.PersistentConfiguration;

using RepoM.Core.Plugin.AssemblyInformation;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
[ModuleConfiguration]
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

    [ModuleConfigurationDefaultValueFactoryMethod]
    internal static SonarCloudConfigV1 CreateDefault()
    {
        return new SonarCloudConfigV1()
        {
            BaseUrl = "https://sonarcloud.io",
            PersonalAccessToken = null,
        };
    }
}