namespace RepoM.Api.Ordering.IsPinned;

using System;
using RepoM.Api.Git;
using RepoM.Core.Plugin.RepositoryOrdering;

public class IsPinnedScorerFactory : IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1>
{
    private readonly IRepositoryMonitor _monitor;

    public IsPinnedScorerFactory(IRepositoryMonitor monitor)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public IRepositoryScoreCalculator Create(IsPinnedScorerConfigurationV1 config)
    {
        return new IsPinnedScoreCalculator(_monitor, config.Weight);
    }
}