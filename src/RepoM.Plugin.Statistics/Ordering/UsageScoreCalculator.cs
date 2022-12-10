namespace RepoM.Plugin.Statistics.Ordering;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class UsageScoreCalculator : IRepositoryScoreCalculator
{
    private readonly StatisticsService _service;
    private readonly IClock _clock;

    public UsageScoreCalculator(StatisticsService service, IClock clock)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public int Score(IRepository repository)
    {
        var now = _clock.Now;

        IReadOnlyRepositoryStatistics? repositoryRecording = _service.GetRepositoryRecording(repository);
        if (repositoryRecording == null)
        {
            return 0;
        }

        var count = repositoryRecording.GetRecordingCount(now.AddDays(5), now);
        return count;

        // IReadOnlyList<DateTime> recordings = _service.GetRecordings(repository);
        // return recordings.Count;
    }
}