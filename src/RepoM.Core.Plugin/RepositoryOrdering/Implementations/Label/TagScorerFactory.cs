namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

using System;

public class TagScorerFactory : IRepositoryScoreCalculatorFactory<TagScorerConfigurationV1>
{
    public IRepositoryScoreCalculator Create(TagScorerConfigurationV1 config)
    {
        if (string.IsNullOrWhiteSpace(config.Tag))
        {
            throw new Exception("Tag cannot be null");
        }

        return new TagScoreCalculator(config.Tag!, config.Weight);
    }
}