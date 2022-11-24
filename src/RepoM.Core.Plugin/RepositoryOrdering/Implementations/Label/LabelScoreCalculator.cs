namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Label;

using System.Linq;

public class LabelScoreCalculator : IRepositoryScoreCalculator
{
    private readonly string _label;
    private readonly int _weight;

    public LabelScoreCalculator(string label, int weight)
    {
        _label = label;
        _weight = weight;
    }

    public int Score(IRepository repository)
    {
        return repository.Tags.Contains(_label) ? _weight : 0;
    }
}