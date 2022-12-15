namespace RepoM.Plugin.Statistics.Ordering;

using System.Collections.Generic;

internal class ScoreCalculatorConfig
{
    public List<ScoreCalculatorRangeConfig> Ranges { get; set; } = new();

    public int MaxScore { get; set; } = int.MaxValue;
}