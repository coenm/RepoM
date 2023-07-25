namespace RepoM.Plugin.AzureDevOps;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Plugin.AzureDevOps.ActionProvider;
using RepoM.Plugin.AzureDevOps.Internal;
using RepoM.Plugin.AzureDevOps.PersistentConfiguration;
using RepoM.Plugin.AzureDevOps.RepositoryFiltering;
using SimpleInjector;

[UsedImplicitly]
public class AzureDevOpsPackage : IPackageWithConfiguration
{
    public string Name => "AzureDevOpsPackage"; // do not change this name, it is part of the persistant filename

    public async Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        await ExtractAndRegisterConfiguration(container, packageConfiguration).ConfigureAwait(false);
        RegisterServices(container);
    }

    private static async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
    {
        var version = await packageConfiguration.GetConfigurationVersionAsync().ConfigureAwait(false);

        AzureDevopsConfigV1 config;
        if (version == CurrentConfigVersion.VERSION)
        {
            AzureDevopsConfigV1? result = await packageConfiguration.LoadConfigurationAsync<AzureDevopsConfigV1>().ConfigureAwait(false);
            config = result ?? new AzureDevopsConfigV1();
        }
        else
        {
            config = new AzureDevopsConfigV1();
            await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        }

        // this is temporarly to support the old way of storing the configuration
        if (string.IsNullOrWhiteSpace(config.BaseUrl) && string.IsNullOrWhiteSpace(config.PersonalAccessToken))
        {
            container.RegisterSingleton<IAzureDevopsConfiguration>(() =>
                {
                    IAppSettingsService appSettingsService = container.GetInstance<IAppSettingsService>();

                    var c = new AzureDevopsConfigV1
                        {
                            PersonalAccessToken = appSettingsService.AzureDevOpsPersonalAccessToken,
                            BaseUrl = appSettingsService.AzureDevOpsBaseUrl,
                        };
                    _ = packageConfiguration.PersistConfigurationAsync(c, CurrentConfigVersion.VERSION); // do not await

                    return new AzureDevopsConfiguration(c.BaseUrl, c.PersonalAccessToken);
                });
        }
        else
        {
            container.RegisterSingleton<IAzureDevopsConfiguration>(() =>
                {
                    IAppSettingsService appSettingsService = container.GetInstance<IAppSettingsService>();

                    appSettingsService.AzureDevOpsBaseUrl = "This value has been copied to the new configuration file for this module.";
                    appSettingsService.AzureDevOpsPersonalAccessToken = "This value has been copied to the new configuration file for this module.";

                    return new AzureDevopsConfiguration(config.BaseUrl, config.PersonalAccessToken);
                });
        }
    }

    private static void RegisterServices(Container container)
    {
        container.Collection.Append<IActionDeserializer, ActionAzureDevOpsCreatePullRequestsV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionDeserializer, ActionAzureDevOpsGetPullRequestsV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionAzureDevOpsCreatePullRequestsV1Mapper>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionAzureDevOpsGetPullRequestsV1Mapper>(Lifestyle.Singleton);

        container.Register<IAzureDevOpsPullRequestService, AzureDevOpsPullRequestService>(Lifestyle.Singleton);

        container.Collection.Append<IModule, AzureDevOpsModule>(Lifestyle.Singleton);
        container.Collection.Append<IQueryMatcher>(() => new HasPullRequestsMatcher(container.GetInstance<IAzureDevOpsPullRequestService>(), true), Lifestyle.Singleton);
    }
}