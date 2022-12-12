namespace RepoM.Plugin.Statistics.Ordering;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public sealed class UsageScorerConfigurationV1 : IRepositoryScorerConfiguration
{
    public List<Windows> Windows { get; set; } = new List<Windows>();

    public int? MaxScore { get; set; } = null;
}

public sealed class Windows
{
    public TimeSpan Until { get; set; }

    public int Weight { get; set; }

    public int MaxItems { get; set; }
}