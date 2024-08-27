namespace RepoM.Api.Common;

using System;
using System.Collections.Generic;
using RepoM.Api.Git.AutoFetch;

/// <summary>
/// RepoM application settings. Most of them are configurable in the UI but not all.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// The selected sorting strategy. Sorting strategies can be configured manually in `RepoM.Ordering.yaml`.
    /// </summary>
    [UiConfigured]
    public string SortKey { get; set; } = string.Empty;

    /// <summary>
    /// The selected query parser. Query parsers can be added by plugins.
    /// </summary>
    [UiConfigured]
    public string SelectedQueryParser { get; set; } = string.Empty;

    /// <summary>
    /// The selected filtering strategy. Filtering strategies can be configured manually in `RepoM.Filtering.yaml`.
    /// </summary>
    [UiConfigured]
    public string SelectedFilter { get; set; } = string.Empty;

    /// <summary>
    /// The git fetching strategy. This determines how often RepoM will fetch from git.
    /// </summary>
    [UiConfigured]
    public AutoFetchMode AutoFetchMode { get; set; } = AutoFetchMode.Off;

    /// <summary>
    /// This option determines if RepoM should prune branches when fetching from git.
    /// </summary>
    [UiConfigured]
    public bool PruneOnFetch { get; set; }

    /// <summary>
    /// Preferred menu sizes of the RepoM. Will be set when window is resized.
    /// </summary>
    [UiConfigured]
    public Dictionary<string, Size> PreferredMenuSizes { get; set; } = new();

    /// <summary>
    /// List of root directories where RepoM will search for git repositories. If null or empty, all fixed drives will be searched from the root.
    /// </summary>
    [ManualConfigured]
    public List<string> ReposRootDirectories { get; set; } = new();

    /// <summary>
    /// List of plugins.
    /// </summary>
    [UiConfigured]
    public List<PluginOptions> Plugins { get; set; } = new();

    public static AppSettings Default => new()
        {
            AutoFetchMode = AutoFetchMode.Off,
            PruneOnFetch = false,
            ReposRootDirectories = new(),
            Plugins = [],
            PreferredMenuSizes = new(),
        };
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class UiConfiguredAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ManualConfiguredAttribute : Attribute { }

public class Size
{
    public double Height { get; init; }

    public double Width { get; init; }
}

public class PluginOptions
{
    public string Name { get; init; } = string.Empty;

    public string DllName { get; init; } = string.Empty;

    public bool Enabled { get; init; } = false;
}