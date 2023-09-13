namespace RepoM.Plugin.WebBrowser.Services;

using System.Collections.Generic;

internal class WebBrowserConfiguration
{
    public Dictionary<string, string> Browsers { get; init; }

    public Dictionary<string, BrowserProfileConfig> Profiles { get; init; }
}

internal class BrowserProfileConfig
{
    public string? BrowserName { get; set; }

    public string? CommandLineArguments { get; set; }
}