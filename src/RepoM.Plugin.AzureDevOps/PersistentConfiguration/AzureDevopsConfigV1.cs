namespace RepoM.Plugin.AzureDevOps.PersistentConfiguration;

using RepoM.Core.Plugin;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
[ModuleConfiguration(VERSION)]
public class AzureDevopsConfigV1
{
    internal const int VERSION = 1;

    /// <summary>
    /// Personal access token (PAT) to access Azure Devops. The PAT should be granted access to read and write pull requests.
    /// To create a PAT, goto `https://dev.azure.com/[my-organisation]/_usersSettings/tokens`.
    /// </summary>
    public string? PersonalAccessToken { get; init; }

    /// <summary>
    /// The base url of azure devops for your organisation (i.e. `https://dev.azure.com/[my-organisation]/`).
    /// </summary>
    public string? BaseUrl { get; init; }

    [ModuleConfigurationDefaultValueFactoryMethod]
    internal static AzureDevopsConfigV1 CreateDefault()
    {
        return new AzureDevopsConfigV1();
    }
}