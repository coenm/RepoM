namespace RepoM.Plugin.WebBrowser.PersistentConfiguration;

using System.Collections.Generic;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
public class WebBrowserConfigV1
{
    /// <summary>
    /// Dictionary of known browsers and their path to use for opening urls.
    /// </summary>
    public Dictionary<string, string>? Browsers { get; set; } = new();

    /// <summary>
    /// Profiles to use. 
    /// </summary>
    public Dictionary<string, ProfileConfig>? Profiles { get; set; } = new();
}