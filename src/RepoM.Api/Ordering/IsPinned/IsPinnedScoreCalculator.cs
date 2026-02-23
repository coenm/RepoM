namespace RepoM.Api.Ordering.IsPinned;

using System;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Repositories.Pinning;

public class IsPinnedScoreCalculator : IRepositoryScoreCalculator
{
    private readonly IPinningService _pinningService;
    private readonly int _weight;

    public IsPinnedScoreCalculator(IPinningService pinningService, int weight)
    {
        _pinningService = pinningService ?? throw new ArgumentNullException(nameof(pinningService));
        _weight = weight;
    }

    public int Score(IRepository repository)
    {
        return _pinningService.IsPinned(repository.SafePath) ? _weight : 0;
    }
}
