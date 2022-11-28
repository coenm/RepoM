namespace RepoM.Plugin.Statistics;

using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.VariableProviders;
using RepoM.Plugin.Statistics.Ordering;
using RepoM.Plugin.Statistics.RepositoryActions;
using RepoM.Plugin.Statistics.VariableProviders;
using SimpleInjector;
using SimpleInjector.Packaging;

[UsedImplicitly]
public class StatisticsPackage : IPackage
{
    public void RegisterServices(Container container)
    {
        RegisterPluginHooks(container);
        RegisterInternals(container);
    }

    private static void RegisterPluginHooks(Container container)
    {
        // ordering
        container.Collection.Append<IConfigurationRegistration, UsageScorerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Register<IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>, UsageScorerFactory>(Lifestyle.Singleton);

        // action executor
        container.RegisterDecorator(typeof(IActionExecutor<>), typeof(RecordStatisticsActionExecutorDecorator<>), Lifestyle.Singleton);

        // variable provider
        container.Collection.Append<IVariableProvider, UsageVariableProvider>(Lifestyle.Singleton);

        // module
        container.Collection.Append<IModule, StatisticsModule>(Lifestyle.Singleton);
    }

    private static void RegisterInternals(Container container)
    {
        container.Register<StatisticsService>(Lifestyle.Singleton);
    }
}