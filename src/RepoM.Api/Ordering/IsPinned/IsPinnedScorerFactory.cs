namespace RepoM.Api.Ordering.IsPinned;

using System;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Repositories.Pinning;

public class IsPinnedScorerFactory : IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1>
{
    private readonly IPinningService _pinningService;

    public IsPinnedScorerFactory(IPinningService pinningService)
    {
        _pinningService = pinningService ?? throw new ArgumentNullException(nameof(pinningService));
    }

    public IRepositoryScoreCalculator Create(IsPinnedScorerConfigurationV1 config)
    {
        return new IsPinnedScoreCalculator(_pinningService, config.Weight);
    }
}
