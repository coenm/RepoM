namespace RepoM.Plugin.Statistics;

using System;

internal class StatisticsConfiguration : IStatisticsConfiguration
{
    public TimeSpan PersistenceBuffer { get; init; }

    public int RetentionDays { get; init; }
}