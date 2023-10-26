namespace RepoM.Plugin.Statistics;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.VariableProviders;
using RepoM.Plugin.Statistics.ActionMenu.Context;
using RepoM.Plugin.Statistics.Ordering;
using RepoM.Plugin.Statistics.PersistentConfiguration;
using RepoM.Plugin.Statistics.RepositoryActions;
using RepoM.Plugin.Statistics.VariableProviders;
using SimpleInjector;

[UsedImplicitly]
public class StatisticsPackage : IPackage
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

        StatisticsConfigV1 config;

        if (version == CurrentConfigVersion.VERSION)
        {
            StatisticsConfigV1? result = await packageConfiguration.LoadConfigurationAsync<StatisticsConfigV1>().ConfigureAwait(false);
            if (result == null)
            {
                config = await PersistDefaultConfigAsync(packageConfiguration).ConfigureAwait(false);
            }
            else
            {
                config = result;
            }
        }
        else
        {
            config = await PersistDefaultConfigAsync(packageConfiguration).ConfigureAwait(false);
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
        container.RegisterScorerConfigurationForType<UsageScorerConfigurationV1>();
        container.Register<IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>, UsageScorerFactory>(Lifestyle.Singleton);

        container.RegisterComparerConfigurationForType<LastOpenedConfigurationV1>();
        container.Register<IRepositoryComparerFactory<LastOpenedConfigurationV1>, LastOpenedComparerFactory>(Lifestyle.Singleton);

        // action executor
        container.RegisterDecorator(typeof(ICommandExecutor<>), typeof(RecordStatisticsCommandExecutorDecorator<>), Lifestyle.Singleton);

        // variable provider
        container.Collection.Append<IVariableProvider, UsageVariableProvider>(Lifestyle.Singleton);

        // module
        container.Collection.Append<IModule, StatisticsModule>(Lifestyle.Singleton);

        container.Collection.Append<ITemplateContextRegistration, UsageVariables>(Lifestyle.Singleton);
    }

    private static void RegisterInternals(Container container)
    {
        container.Register<IStatisticsService, StatisticsService>(Lifestyle.Singleton);
    }

    /// <remarks>This method is used by reflection to generate documentation file</remarks>
    private static async Task<StatisticsConfigV1> PersistDefaultConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = new StatisticsConfigV1
            {
                PersistenceBuffer = TimeSpan.FromMinutes(5),
                RetentionDays = 30,
            };
        await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        return config;
    }
}