namespace RepoM.Plugin.Statistics;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.VariableProviders;
using RepoM.Plugin.Statistics.Ordering;
using RepoM.Plugin.Statistics.PersistentConfiguration;
using RepoM.Plugin.Statistics.RepositoryActions;
using RepoM.Plugin.Statistics.VariableProviders;
using SimpleInjector;

[UsedImplicitly]
public class StatisticsPackage : IPackageWithConfiguration
{
    public string Name => "StatisticsPackage"; // do not change this name, it is part of the persistant filename

    public async Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        await ExtractAndRegisterConfiguration(container, packageConfiguration).ConfigureAwait(false);
        RegisterPluginHooks(container);
        RegisterInternals(container);
    }

    private static async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
    {
        var version = await packageConfiguration.GetConfigurationVersionAsync().ConfigureAwait(false);

        var config = new StatisticsConfigV1
            {
                PersistenceBuffer = TimeSpan.FromMinutes(5),
                RetentionDays = 30,
            };

        if (version == CurrentConfigVersion.VERSION)
        {
            StatisticsConfigV1? result = await packageConfiguration.LoadConfigurationAsync<StatisticsConfigV1>().ConfigureAwait(false);
            if (result == null)
            {
                await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
            }
            else
            {
                config = result;
            }
        }
        else
        {
            await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        }

        var retentionDays = config.RetentionDays ?? 30;
        if (retentionDays < 0)
        {
            retentionDays *= -1;
        }

        container.RegisterInstance<IStatisticsConfiguration>(
            new StatisticsConfiguration
            {
                PersistenceBuffer = config.PersistenceBuffer ?? TimeSpan.FromMinutes(5),
                RetentionDays= retentionDays,
            });
    }

    private static void RegisterPluginHooks(Container container)
    {
        // ordering
        container.Collection.Append<IConfigurationRegistration, UsageScorerConfigurationV1Registration>(Lifestyle.Singleton);
        container.Register<IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>, UsageScorerFactory>(Lifestyle.Singleton);

        container.Collection.Append<IConfigurationRegistration, LastOpenedConfigurationV1Registration>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<LastOpenedConfigurationV1>, LastOpenedComparerFactory>(Lifestyle.Singleton);

        // action executor
        container.RegisterDecorator(typeof(IActionExecutor<>), typeof(RecordStatisticsActionExecutorDecorator<>), Lifestyle.Singleton);

        // variable provider
        container.Collection.Append<IVariableProvider, UsageVariableProvider>(Lifestyle.Singleton);

        // module
        container.Collection.Append<IModule, StatisticsModule>(Lifestyle.Singleton);
    }

    private static void RegisterInternals(Container container)
    {
        container.Register<IStatisticsService, StatisticsService>(Lifestyle.Singleton);
    }
}