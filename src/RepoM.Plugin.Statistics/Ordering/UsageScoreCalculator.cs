namespace RepoM.Plugin.Statistics.Ordering;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class UsageScoreCalculator : IRepositoryScoreCalculator
{
    private readonly StatisticsService _service;

    public UsageScoreCalculator(StatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public int Score(IRepository repository)
    {
        IReadOnlyList<DateTime> recordings = _service.GetRecordings(repository);
        return recordings.Count;
    }
}