namespace RepoM.Plugin.Statistics;

using System;

public interface IReadOnlyRepositoryStatistics
{
    int GetRecordingCount(DateTime from, DateTime to);
    int GetRecordingCountFrom(DateTime from);
    int GetRecordingCountBefore(DateTime to);
}