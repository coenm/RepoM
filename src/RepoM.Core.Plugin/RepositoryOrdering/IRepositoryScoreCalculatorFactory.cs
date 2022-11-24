namespace RepoM.Core.Plugin.RepositoryOrdering;

using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public interface IRepositoryScoreCalculatorFactory
{
    IRepositoryScoreCalculator Create(IRepositoryScorerConfiguration configuration);
}

public interface IRepositoryScoreCalculatorFactory<in TConfig>
    where TConfig : IRepositoryScorerConfiguration
{
    IRepositoryScoreCalculator Create(TConfig config);
}