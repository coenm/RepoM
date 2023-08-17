namespace RepoM.Plugin.AzureDevOps;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

    private static async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
    {
        var version = await packageConfiguration.GetConfigurationVersionAsync().ConfigureAwait(false);

        AzureDevopsConfigV2? config = null!;

        if (version == AzureDevopsConfigV1.VERSION)
        {
            AzureDevopsConfigV1? configV1 = await packageConfiguration.LoadConfigurationAsync<AzureDevopsConfigV1>().ConfigureAwait(false);
            config = ConvertV1ToV2(configV1);
            await packageConfiguration.PersistConfigurationAsync(config, AzureDevopsConfigV2.VERSION).ConfigureAwait(false);
        }
        else if (version == AzureDevopsConfigV2.VERSION)
        {
            config = await packageConfiguration.LoadConfigurationAsync<AzureDevopsConfigV2>().ConfigureAwait(false);
        }

        config ??= await PersistDefaultConfigAsync(packageConfiguration).ConfigureAwait(false);

        container.RegisterInstance<IAzureDevopsConfiguration>(new AzureDevopsConfiguration(
            config.BaseUrl,
            config.PersonalAccessToken,
            config.DefaultProjectId,
            config.IntervalUpdateProjects ?? TimeSpan.FromMinutes(10),
            config.IntervalUpdatePullRequests ?? TimeSpan.FromMinutes(4)));
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
    private static async Task<AzureDevopsConfigV2> PersistDefaultConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = CreateDefaultAzureDevopsConfigV2();
        await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        return config;
    }

    private static AzureDevopsConfigV2 CreateDefaultAzureDevopsConfigV2()
    {
        return new AzureDevopsConfigV2
            {
                BaseUrl = null,
                PersonalAccessToken = null,
                DefaultProjectId = null,
                IntervalUpdateProjects = TimeSpan.FromMinutes(10),
                IntervalUpdatePullRequests = TimeSpan.FromMinutes(4),
            };
    }

    private static AzureDevopsConfigV2 ConvertV1ToV2(AzureDevopsConfigV1? configV1)
    {
        AzureDevopsConfigV2 defaultConfig = CreateDefaultAzureDevopsConfigV2();
        return new AzureDevopsConfigV2
            {
                BaseUrl = configV1?.BaseUrl,
                PersonalAccessToken = configV1?.PersonalAccessToken,
                DefaultProjectId = defaultConfig.DefaultProjectId,
                IntervalUpdateProjects = defaultConfig.IntervalUpdateProjects,
                IntervalUpdatePullRequests = defaultConfig.IntervalUpdatePullRequests,
            };
    }
}