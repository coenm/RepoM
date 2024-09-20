namespace RepoM.Plugin.WebBrowser.PersistentConfiguration;

using System.Collections.Generic;
using RepoM.Core.Plugin;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
[ModuleConfiguration(VERSION)]
public class WebBrowserConfigV1
{
    internal const int VERSION = 1;

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

    [ModuleConfigurationExampleValueFactoryMethod]
    internal static WebBrowserConfigV1 CreateExample()
    {
        return new WebBrowserConfigV1
        {
            Browsers = new Dictionary<string, string>
            {
                { "Edge", @"C:\PathTo\msedge.exe" },
                { "FireFox", @"C:\PathTo\Mozilla\firefox.exe" },
            },
            Profiles = new Dictionary<string, ProfileConfig>
            {
                { "Work", new ProfileConfig { BrowserName = "Edge", CommandLineArguments = "\"--profile-directory=Profile 4\" {url}", } },
                { "Incognito", new ProfileConfig { BrowserName = "Edge", CommandLineArguments = "-inprivate", } },
                { "Incognito2", new ProfileConfig { BrowserName = "FireFox", CommandLineArguments = "-inprivate {url}", } },
            },
        };
    }
}