namespace RepoM.Plugin.Statistics.PersistentConfiguration;

using System;

/// <remarks>DO NOT CHANGE PROPERTYNAMES, TYPES, or VISIBILITIES</remarks>
/// <summary>Module configuration (version 1)</summary>
public class StatisticsConfigV1
{
    /// <summary>
    /// Timespan for buffered events before making them persistant (ie. `00:05:00` for five minutes). Must be greater then or equal to `00:00:10` (10 seconds).
    /// </summary>
    public TimeSpan? PersistenceBuffer { get; init; }

    /// <summary>
    /// Number of days to keep statical information before deleting them. 
    /// </summary>
    public int? RetentionDays { get; init; }
}