namespace RepoM.Plugin.Statistics.Ordering;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

/// <summary>
/// Repository scorer based on it's usage by RepoM. The more it's used, the higher the score.
/// </summary>
public sealed class UsageScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public const string TYPE_VALUE = "usage-scorer@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Specific 'windows' to calculate the score for. 
    /// </summary>
    public List<Window> Windows { get; set; } = new List<Window>();

    /// <summary>
    /// The maximum score a repository can get.
    /// </summary>
    public int? MaxScore { get; set; } = null;
}

/// <summary>
/// Window specifies the range for a given weight.
/// </summary>
public sealed class Window
{
    /// <summary>
    /// Timespan for the given window.
    /// </summary>
    public TimeSpan Until { get; set; }

    /// <summary>
    /// The weight used for this window.
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// Maximum number of actions performed to be taken into account for this window. The rest will be used in the next window.
    /// </summary>
    public int MaxItems { get; set; }
}