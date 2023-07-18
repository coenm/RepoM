namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using RepoM.Api.Git.AutoFetch;
using RepoM.Core.Plugin.RepositoryFiltering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public interface ICompareSettingsService
{
    Dictionary<string, IRepositoriesComparerConfiguration> Configuration { get; }
}

public interface IFilterSettingsService
{
    Dictionary<string, RepositoryFilterConfiguration> Configuration { get; }
}

public interface IAppSettingsService
{
    AutoFetchMode AutoFetchMode { get; set; }

    bool PruneOnFetch { get; set; }

    double MenuWidth { get; set; }

    double MenuHeight { get; set; }

    List<string> ReposRootDirectories { get; set; }

    List<string> EnabledSearchProviders { get; set; }

    [Obsolete("Will be removed in next big version")]
    string SonarCloudPersonalAccessToken { get; set; }

    [Obsolete("Will be removed in next big version")]
    string AzureDevOpsPersonalAccessToken { get; set; }

    [Obsolete("Will be removed in next big version")]
    string AzureDevOpsBaseUrl { get; set; }

    string SortKey { get; set; }

    string QueryParserKey { get; set; }

    string SelectedFilter { get; set; }

    public List<PluginSettings> Plugins { get; set; }

    void RegisterInvalidationHandler(Action handler);
}

public class PluginSettings
{
    public PluginSettings(string name, string dllName, bool enabled)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        DllName = dllName ?? throw new ArgumentNullException(nameof(dllName));
        Enabled = enabled;
    }

    public string Name { get; }

    public string DllName { get; }
    
    public bool Enabled { get; set; }
}
