namespace RepoM.Api.Common.Common;

using System.Collections.Generic;
using System.Net;
using RepoM.Api.Common.Git.AutoFetch;

public class AppSettings
{
    public AppSettings()
    {
        MenuSize = Size.Default;
        EnabledSearchProviders = new List<string>();
        SonarCloudPersonalAccessToken = string.Empty;
        AzureDevOps = AzureDevOpsOptions.Default;
    }

    public AutoFetchMode AutoFetchMode { get; set; }

    public bool PruneOnFetch { get; set; }

    public Size MenuSize { get; set; }

    public List<string> EnabledSearchProviders { get; set; }

    public string SonarCloudPersonalAccessToken { get; set; }

    public AzureDevOpsOptions AzureDevOps { get; set; }

    public static AppSettings Default => new()
        {
            AutoFetchMode = AutoFetchMode.Off,
            PruneOnFetch = false,
            MenuSize = Size.Default,
            EnabledSearchProviders = new List<string>(1),
            SonarCloudPersonalAccessToken = string.Empty,
            AzureDevOps = AzureDevOpsOptions.Default,
    };
}

public class AzureDevOpsOptions
{
    public string PersonalAccessToken { get; set; }

    public string BaseUrl { get; set; }

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