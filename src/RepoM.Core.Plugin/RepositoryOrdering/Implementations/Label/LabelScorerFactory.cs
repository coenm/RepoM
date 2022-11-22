namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

public class LabelScorerFactory : IRepositoryScoreCalculatorFactory<LabelScorerConfigurationV1>
{
    public IRepositoryScoreCalculator Create(LabelScorerConfigurationV1 config)
    {
        return new LabelScoreCalculator(config.Weight);
    }
}