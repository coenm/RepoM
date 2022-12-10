namespace RepoM.Plugin.Statistics;

using System;

internal interface IReadOnlyRepositoryStatistics
{
    int GetRecordingCount(DateTime from, DateTime to);
    int GetRecordingCountFrom(DateTime from);
    int GetRecordingCountBefore(DateTime to);
}