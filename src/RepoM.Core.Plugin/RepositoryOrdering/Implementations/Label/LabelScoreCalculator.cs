namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

public class LabelScoreCalculator : IRepositoryScoreCalculator
{
    private readonly int _weight;

    public LabelScoreCalculator(int weight)
    {
        _weight = weight;
    }

    public int Score(IPluginRepository repository)
    {
        return 0;
    }
}