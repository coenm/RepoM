namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.IsPinned;

public class IsPinnedScoreCalculator : IRepositoryScoreCalculator
{
    private readonly int _weight;

    public IsPinnedScoreCalculator(int weight)
    {
        _weight = weight;
    }

    public int Score(IPluginRepository repository)
    {
        return repository.IsPinned ? _weight : 0;
    }
}