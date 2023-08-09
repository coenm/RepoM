namespace RepoM.Plugin.AzureDevOps;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Plugin.AzureDevOps.ActionProvider;
using RepoM.Plugin.AzureDevOps.ActionProvider.Options;
using RepoM.Plugin.AzureDevOps.Internal;
using RepoM.Plugin.AzureDevOps.PersistentConfiguration;
using RepoM.Plugin.AzureDevOps.RepositoryFiltering;
using SimpleInjector;

[UsedImplicitly]
public class AzureDevOpsPackage : IPackage
{
    public string Name => "AzureDevOpsPackage"; // do not change this name, it is part of the persistant filename

    public async Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        await ExtractAndRegisterConfiguration(container, packageConfiguration).ConfigureAwait(false);
        RegisterServices(container);
    }

    private async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
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
            config = await PersistDefaultConfigAsync(packageConfiguration).ConfigureAwait(false);
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
        container.RegisterDefaultRepositoryActionDeserializerForType<RepositoryActionAzureDevOpsCreatePullRequestsV1>();
        container.RegisterDefaultRepositoryActionDeserializerForType<RepositoryActionAzureDevOpsGetPullRequestsV1>();

        container.Collection.Append<IActionToRepositoryActionMapper, ActionAzureDevOpsCreatePullRequestsV1Mapper>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionAzureDevOpsGetPullRequestsV1Mapper>(Lifestyle.Singleton);

        container.Register<IAzureDevOpsPullRequestService, AzureDevOpsPullRequestService>(Lifestyle.Singleton);

        container.Collection.Append<IModule, AzureDevOpsModule>(Lifestyle.Singleton);
        container.Collection.Append<IQueryMatcher>(() => new HasPullRequestsMatcher(container.GetInstance<IAzureDevOpsPullRequestService>(), true), Lifestyle.Singleton);
    }

    /// <remarks>This method is used by reflection to generate documentation file</remarks>>
    private static async Task<AzureDevopsConfigV1> PersistDefaultConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = new AzureDevopsConfigV1();
        await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        return config;
    }
}