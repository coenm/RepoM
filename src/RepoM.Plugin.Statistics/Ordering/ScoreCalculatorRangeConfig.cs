namespace RepoM.Plugin.Statistics.Ordering;

using System;

internal class ScoreCalculatorRangeConfig
{
    public TimeSpan MaxAge { get; set; }

    public int Score { get; set;}

    public int MaxItems { get; set; } = int.MaxValue;
}