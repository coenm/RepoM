namespace RepoM.Core.Plugin.RepositoryOrdering;

public interface IRepositoryScoreCalculator
{
    int Score(IPluginRepository repository);
}