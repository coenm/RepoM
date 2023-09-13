namespace RepoM.Plugin.WebBrowser.PersistentConfiguration;

public class ProfileConfig
{
    /// <summary>
    /// Name of the browser. Should be listed in the Browsers dictionary.
    /// </summary>
    public string? BrowserName { get; set; }

    /// <summary>
    /// Command line arguments
    /// </summary>
    public string? CommandLineArguments { get; set; }
}