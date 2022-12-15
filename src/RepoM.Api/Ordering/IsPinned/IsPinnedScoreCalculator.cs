namespace RepoM.Api.Ordering.IsPinned;

using System;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;

public class IsPinnedScoreCalculator : IRepositoryScoreCalculator
{
    private readonly IRepositoryMonitor _monitor;
    private readonly int _weight;

    public IsPinnedScoreCalculator(IRepositoryMonitor monitor, int weight)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _weight = weight;
    }

    public int Score(IRepository repository)
    {
        if (repository is Repository r)
        {
            return _monitor.IsPinned(r) ? _weight : 0;
        }

        return 0;
    }
}