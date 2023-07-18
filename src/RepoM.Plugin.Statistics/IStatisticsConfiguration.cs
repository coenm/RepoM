namespace RepoM.Plugin.Statistics;

using System;

internal interface IStatisticsConfiguration
{
    public TimeSpan PersistenceBuffer { get; }

    public int RetentionDays { get; }
}