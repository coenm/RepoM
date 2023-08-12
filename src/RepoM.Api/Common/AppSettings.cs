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
    public bool PruneOnFetch { get; set; } = false;

    /// <summary>
    /// The menu size of RepoM. This is set when the window is resized.
    /// </summary>
    [UiConfigured]
    public Size MenuSize { get; set; } = Size.Default;

    /// <summary>
    /// List of root directories where RepoM will search for git repositories. If null or empty, all fixed drives will be searched from the root.
    /// </summary>
    [ManualConfigured]
    public List<string> ReposRootDirectories { get; set; } = new();

    /// <summary>
    /// List of search providers. Search providers can be added by plugins.
    /// </summary>
    [UiConfigured]
    public List<string> EnabledSearchProviders { get; set; } = new();

    // no xml docs because it is obsolete, this way, it will not appear in the docs.
    [Obsolete("This is done using plugin.")]
    public string? SonarCloudPersonalAccessToken { get; set; } = string.Empty;

    // no xml docs because it is obsolete, this way, it will not appear in the docs.
    [Obsolete("This is done using plugin.")]
    public AzureDevOpsOptions? AzureDevOps { get; set; } = AzureDevOpsOptions.Default;

    /// <summary>
    /// List of plugins.
    /// </summary>
    [UiConfigured]
    public List<PluginOptions> Plugins { get; set; } = new();

    public static AppSettings Default => new()
        {
            AutoFetchMode = AutoFetchMode.Off,
            PruneOnFetch = false,
            MenuSize = Size.Default,
            ReposRootDirectories = new(),
            EnabledSearchProviders = new List<string>(1),
            SonarCloudPersonalAccessToken = null,
            AzureDevOps = AzureDevOpsOptions.Default,
            Plugins = new List<PluginOptions>(),
        };
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class UiConfiguredAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ManualConfiguredAttribute : Attribute { }

public class AzureDevOpsOptions
{
    public string? PersonalAccessToken { get; set; } = string.Empty;

    public string? BaseUrl { get; set; } = string.Empty;

    public static AzureDevOpsOptions? Default => null;
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