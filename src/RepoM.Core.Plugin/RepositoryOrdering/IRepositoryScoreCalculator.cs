namespace RepoM.Core.Plugin.RepositoryOrdering;

using RepoM.Core.Plugin.Repository;

public interface IRepositoryScoreCalculator
{
    int Score(IRepository repository);
}