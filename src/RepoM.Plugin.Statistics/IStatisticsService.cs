namespace RepoM.Plugin.Statistics;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Statistics.Interface;

public interface IStatisticsService
{
    IObservable<IEvent> Events { get; }

    void Apply(IEvent evt);

    void Record(IRepository repository);

    int GetTotalRecordingCount();

    IReadOnlyList<DateTime> GetRecordings(IRepository repository);

    IReadOnlyRepositoryStatistics? GetRepositoryRecording(IRepository repository);
}