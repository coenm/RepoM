namespace RepoM.Plugin.WebBrowser.Services;

using System.Collections.Generic;

internal class WebBrowserConfiguration
{
    public Dictionary<string, string> Browsers { get; init; } = new();

    public Dictionary<string, BrowserProfileConfig> Profiles { get; init; } = new();
}

internal class BrowserProfileConfig
{
    public string? BrowserName { get; set; }

    public string? CommandLineArguments { get; set; }
}