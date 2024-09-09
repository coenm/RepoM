namespace RepoM.Plugin.WebBrowser.PersistentConfiguration;

using System.Collections.Generic;
using RepoM.Core.Plugin.AssemblyInformation;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
[ModuleConfiguration]
public class WebBrowserConfigV1
{
    /// <summary>
    /// Dictionary of known browsers and their path to use for opening urls.
    /// </summary>
    public Dictionary<string, string>? Browsers { get; set; }

    /// <summary>
    /// Profiles to use. 
    /// </summary>
    public Dictionary<string, ProfileConfig>? Profiles { get; set; }

    [ModuleConfigurationDefaultValueFactoryMethod]
    internal static WebBrowserConfigV1 CreateDefault()
    {
        return new WebBrowserConfigV1
        {
            Browsers = null,
            Profiles = null,
        };
    }
}