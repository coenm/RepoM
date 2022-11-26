namespace RepoM.Api.Ordering.Score;

using System;
using RepoM.Core.Plugin.RepositoryOrdering;

public class ScoreRepositoryComparerFactory : IRepositoryComparerFactory<ScoreComparerConfigurationV1>
{
    private readonly IRepositoryScoreCalculatorFactory _factory;

    public ScoreRepositoryComparerFactory(IRepositoryScoreCalculatorFactory factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IRepositoryComparer Create(ScoreComparerConfigurationV1 configuration)
    {
        return new ScoreComparer(_factory.Create(configuration.ScoreProvider!));
    }
}