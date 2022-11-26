namespace RepoM.Api.Ordering.Label;

using System.Linq;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryOrdering;

public class TagScoreCalculator : IRepositoryScoreCalculator
{
    private readonly string _tag;
    private readonly int _weight;

    public TagScoreCalculator(string tag, int weight)
    {
        _tag = tag;
        _weight = weight;
    }

    public int Score(IRepository repository)
    {
        return repository.Tags.Contains(_tag) ? _weight : 0;
    }
}