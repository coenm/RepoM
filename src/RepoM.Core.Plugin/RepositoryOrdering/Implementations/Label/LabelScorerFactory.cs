namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

using System;

public class LabelScorerFactory : IRepositoryScoreCalculatorFactory<LabelScorerConfigurationV1>
{
    public IRepositoryScoreCalculator Create(LabelScorerConfigurationV1 config)
    {
        if (string.IsNullOrWhiteSpace(config.Label))
        {
            throw new Exception("Label cannot be null");
        }

        return new LabelScoreCalculator(config.Label!, config.Weight);
    }
}