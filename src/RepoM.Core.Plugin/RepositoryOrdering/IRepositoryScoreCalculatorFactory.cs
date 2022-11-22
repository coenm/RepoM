namespace RepoM.Core.Plugin.RepositoryOrdering;

using System;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering.Implementations;

public interface IRepositoryScoreCalculatorFactory<in TConfig, out TImplementation>
    where TImplementation : class, IRepositoryScoreCalculator
    where TConfig : IRepositoryScorerConfiguration
{
    TImplementation Create(TConfig config);
}

public class IsPinnedScorerFactory : IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1, IsPinnedScoreCalculator>
{
    public IsPinnedScoreCalculator Create(IsPinnedScorerConfigurationV1 config)
    {
        return new IsPinnedScoreCalculator(config.Weight);
    }
}