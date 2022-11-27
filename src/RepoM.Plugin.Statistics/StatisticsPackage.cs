namespace RepoM.Plugin.Statistics;

using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Plugin.Statistics.Ordering;
using RepoM.Plugin.Statistics.RepositoryActions;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class StatisticsPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        // container.Collection.Append<IConfigurationRegistration, UsageScorerConfigurationV1Registration>(Lifestyle.Singleton);

        // container.Collection.Append<IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>, IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>>(Lifestyle.Singleton);

        // container.Collection.Append<IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>, UsageScorerFactory>(Lifestyle.Singleton);
        // container.Register<IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>, UsageScorerFactory>(Lifestyle.Singleton);

        container.RegisterDecorator(
            typeof(IActionExecutor<>),
            typeof(RecordStatisticsActionExecutorDecorator<>),
            Lifestyle.Singleton);

        container.Register<StatisticsService>(Lifestyle.Singleton);
        container.Collection.Append<IModule, StatisticsModule>(Lifestyle.Singleton);
    }
}