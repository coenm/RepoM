namespace RepoM.Api.Common;

using System.Collections.Generic;
using RepoM.Api.Git.AutoFetch;

public class AppSettings
{
    public AppSettings()
    {
        MenuSize = Size.Default;
        ReposRootDirectories= new List<string>();
        EnabledSearchProviders = new List<string>();
        SonarCloudPersonalAccessToken = string.Empty;
        AzureDevOps = AzureDevOpsOptions.Default;
        SortKey = string.Empty;
        SelectedQueryParser = string.Empty;
        SelectedFilter = string.Empty;
        Plugins = new List<PluginOptions>();
    }
    public string SortKey { get; set; }

    public string SelectedQueryParser { get; set; }

    public string SelectedFilter { get; set; }

    public AutoFetchMode AutoFetchMode { get; set; }

    public bool PruneOnFetch { get; set; }

    public Size MenuSize { get; set; }

    public List<string> ReposRootDirectories { get; set; }

    public List<string> EnabledSearchProviders { get; set; }

    public string SonarCloudPersonalAccessToken { get; set; }

    public AzureDevOpsOptions AzureDevOps { get; set; }

    public List<PluginOptions> Plugins { get; set; } 

    public static AppSettings Default => new()
        {
            AutoFetchMode = AutoFetchMode.Off,
            PruneOnFetch = false,
            MenuSize = Size.Default,
            ReposRootDirectories = new(),
            EnabledSearchProviders = new List<string>(1),
            SonarCloudPersonalAccessToken = string.Empty,
            AzureDevOps = AzureDevOpsOptions.Default,
            Plugins = new List<PluginOptions>(),
        };
}

public class AzureDevOpsOptions
{
    public string PersonalAccessToken { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public static AzureDevOpsOptions Default => new()
        {
            PersonalAccessToken = string.Empty,
            BaseUrl = string.Empty,
        };
}

public class Size
{
    public double Height { get; set; }

    public double Width { get; set; }

    public static Size Default => new()
        {
            Width = -1,
            Height = -1,
        };
}

public class PluginOptions
{
    public PluginOptions()
    {
        Name = string.Empty;
        DllName = string.Empty;
        Enabled = false;
    }

    public string Name { get; init; }

    public string DllName { get; init; }

    public bool Enabled { get; init; }
}