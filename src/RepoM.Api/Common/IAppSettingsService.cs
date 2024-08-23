namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    void UpdateMenuSize(string resolution, MenuSize size);

    bool TryGetMenuSize(string resolution, [NotNullWhen(true)] out MenuSize? size);

    List<string> ReposRootDirectories { get; set; }

    string SortKey { get; set; }

    string QueryParserKey { get; set; }

    string SelectedFilter { get; set; }

    public List<PluginSettings> Plugins { get; set; }

    void RegisterInvalidationHandler(Action handler);
}

public readonly struct MenuSize
{
    public double MenuWidth { get; init; }

    public double MenuHeight { get; init; }
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
