namespace RepoM.Plugin.Statistics.Ordering;

using System;
using RepoM.Core.Plugin.RepositoryOrdering;

public sealed class UsageScorerFactory : IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>
{
    private readonly StatisticsService _service;

    public UsageScorerFactory(StatisticsService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public IRepositoryScoreCalculator Create(UsageScorerConfigurationV1 config)
    {
        return new UsageScoreCalculator(_service);
    }
}