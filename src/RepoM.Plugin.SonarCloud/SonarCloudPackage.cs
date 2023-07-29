namespace RepoM.Plugin.SonarCloud;

using System.Threading.Tasks;
using ExpressionStringEvaluator.Methods;
using JetBrains.Annotations;
using RepoM.Api.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Plugin.SonarCloud.PersistentConfiguration;
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

        SonarCloudConfigV1 config;
        if (version == CurrentConfigVersion.VERSION)
        {
            SonarCloudConfigV1? result = await packageConfiguration.LoadConfigurationAsync<SonarCloudConfigV1>().ConfigureAwait(false);
            config = result ?? new SonarCloudConfigV1();
        }
        else
        {
            config = await PersistDefaultConfigAsync(packageConfiguration).ConfigureAwait(false);
        }

        // this is temporarly to support the old way of storing the configuration
        if (string.IsNullOrWhiteSpace(config.PersonalAccessToken))
        {
            container.RegisterSingleton<ISonarCloudConfiguration>(() =>
                {
                    IAppSettingsService appSettingsService = container.GetInstance<IAppSettingsService>();

                    var c = new SonarCloudConfigV1
                        {
                            PersonalAccessToken = appSettingsService.SonarCloudPersonalAccessToken,
                            BaseUrl = config.BaseUrl,
                        };
                   _ = packageConfiguration.PersistConfigurationAsync(c, CurrentConfigVersion.VERSION); // fire and forget ;-)

                    return new SonarCloudConfiguration(c.BaseUrl, c.PersonalAccessToken);
                });
        }
        else
        {
            container.RegisterSingleton<ISonarCloudConfiguration>(() =>
                {
                    IAppSettingsService appSettingsService = container.GetInstance<IAppSettingsService>();
                    appSettingsService.SonarCloudPersonalAccessToken = "This value has been copied to the new configuration file for this module.";
                    return new SonarCloudConfiguration(config.BaseUrl, config.PersonalAccessToken);
                });
        }
    }

    private static void RegisterServices(Container container)
    {
        container.Collection.Append<IActionDeserializer, ActionSonarCloudSetFavoriteV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionSonarCloudV1Mapper>(Lifestyle.Singleton);
        container.Collection.Append<IMethod, SonarCloudIsFavoriteMethod>(Lifestyle.Singleton);
        container.Register<ISonarCloudFavoriteService, SonarCloudFavoriteService>(Lifestyle.Singleton);
        container.Collection.Append<IModule, SonarCloudModule>(Lifestyle.Singleton);
    }

    /// <remarks>This method is used by reflection to generate documentation file</remarks>>
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