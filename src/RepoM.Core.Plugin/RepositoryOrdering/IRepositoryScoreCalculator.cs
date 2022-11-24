namespace RepoM.Core.Plugin.RepositoryOrdering;

public interface IRepositoryScoreCalculator
{
    int Score(IRepository repository);
}