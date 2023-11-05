namespace RepoM.Plugin.SonarCloud;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.SonarCloud.ActionMenu.Context;
using RepoM.Plugin.SonarCloud.PersistentConfiguration;
using RepoM.Plugin.SonarCloud.RepositoryCommands;
using SimpleInjector;

[UsedImplicitly]
public class SonarCloudPackage : IPackage
{
    public string Name => "SonarCloudPackage"; // do not change this name, it is part of the persistant filename

    public async Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        await ExtractAndRegisterConfiguration(container, packageConfiguration).ConfigureAwait(false);
        RegisterServices(container);
    }

    private static async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
    {
        var version = await packageConfiguration.GetConfigurationVersionAsync().ConfigureAwait(false);

        SonarCloudConfigV1? config = null;
        if (version == CurrentConfigVersion.VERSION)
        {
            config = await packageConfiguration.LoadConfigurationAsync<SonarCloudConfigV1>().ConfigureAwait(false);
        }

        config ??= await PersistDefaultConfigAsync(packageConfiguration).ConfigureAwait(false);

        container.RegisterInstance<ISonarCloudConfiguration>(new SonarCloudConfiguration(config.BaseUrl, config.PersonalAccessToken));
    }

    private static void RegisterServices(Container container)
    {
        // new style
        container.RegisterActionMenuType<ActionMenu.Model.ActionMenus.SetFavorite.RepositoryActionSonarCloudSetFavoriteV1>();
        container.RegisterActionMenuMapper<ActionMenu.Model.ActionMenus.SetFavorite.RepositoryActionSonarCloudSetFavoriteV1Mapper>(Lifestyle.Singleton);

        // old style
        container.RegisterDefaultRepositoryActionDeserializerForType<RepositoryActionSonarCloudSetFavoriteV1>();
        container.Collection.Append<IActionToRepositoryActionMapper, ActionSonarCloudV1Mapper>(Lifestyle.Singleton);

        // others
        container.Register<ISonarCloudFavoriteService, SonarCloudFavoriteService>(Lifestyle.Singleton);
        container.Collection.Append<IModule, SonarCloudModule>(Lifestyle.Singleton);

        // action executor
        container.Register<ICommandExecutor<SonarCloudSetFavoriteRepositoryCommand>, SonarCloudSetFavoriteRepositoryCommandExecutor>(Lifestyle.Singleton);

        container.Collection.Append<ITemplateContextRegistration, SonarCloudVariables>(Lifestyle.Singleton);
    }

    /// <remarks>This method is used by reflection to generate documentation file</remarks>
    private static async Task<SonarCloudConfigV1> PersistDefaultConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = new SonarCloudConfigV1()
            {
                BaseUrl = "https://sonarcloud.io",
                PersonalAccessToken = null,
            };
        await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        return config;
    }
}