namespace RepoM.Plugin.AzureDevOps;

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

    private async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
    {
        var version = await packageConfiguration.GetConfigurationVersionAsync().ConfigureAwait(false);

        AzureDevopsConfigV1? config = null;
        if (version == CurrentConfigVersion.VERSION)
        {
            config = await packageConfiguration.LoadConfigurationAsync<AzureDevopsConfigV1>().ConfigureAwait(false);
        }

        config ??= await PersistDefaultConfigAsync(packageConfiguration).ConfigureAwait(false);

        container.RegisterInstance<IAzureDevopsConfiguration>(new AzureDevopsConfiguration(config.BaseUrl, config.PersonalAccessToken));
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