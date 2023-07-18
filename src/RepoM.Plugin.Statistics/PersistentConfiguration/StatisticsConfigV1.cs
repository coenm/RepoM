namespace RepoM.Plugin.Statistics.PersistentConfiguration;

using System;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
public class StatisticsConfigV1
{
    public TimeSpan? PersistenceBuffer { get; init; }

    public int? RetentionDays { get; init; }
}