namespace RepoM.Plugin.Statistics.Ordering;

using System;
using RepoM.Api.Git;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class UsageScorerFactory : IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>
{
    private readonly StatisticsService _service;

    public UsageScorerFactory(StatisticsService monitor)
    {
        _service = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public IRepositoryScoreCalculator Create(UsageScorerConfigurationV1 config)
    {
        return new UsageScoreCalculator(_service);
    }
}