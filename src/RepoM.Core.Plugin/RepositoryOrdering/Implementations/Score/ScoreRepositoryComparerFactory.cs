namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Score;

using RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;

public class ScoreRepositoryComparerFactory : IRepositoryComparerFactory<ScoreComparerConfigurationV1>
{
    public IRepositoryComparer Create(ScoreComparerConfigurationV1 configuration)
    {
        // configuration.ScoreProvider

        IRepositoryScoreCalculator calculator = new IsPinnedScoreCalculator(2);
        return new ScoreComparer(calculator);
    }
}