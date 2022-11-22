namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;

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