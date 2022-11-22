namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;

public class IsPinnedScorerFactory : IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1>
{
    public IRepositoryScoreCalculator Create(IsPinnedScorerConfigurationV1 config)
    {
        return new IsPinnedScoreCalculator(config.Weight);
    }
}