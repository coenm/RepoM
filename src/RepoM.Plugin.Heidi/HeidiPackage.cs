namespace RepoM.Plugin.Heidi;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.VariableProviders;
using RepoM.Plugin.Heidi.Internal;
using SimpleInjector;
using RepoM.Plugin.Heidi.VariableProviders;
using RepoM.Plugin.Heidi.ActionProvider;
using SimpleInjector.Packaging;
using RepoM.Plugin.Heidi.PersistentConfiguration;

[UsedImplicitly]
public class HeidiPackage : IPackageWithConfiguration
{
    public string Name => "HeidiPackage"; // do not change this name, it is part of the persistant filename

    public async Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        await ExtractAndRegisterConfiguration(container, packageConfiguration).ConfigureAwait(false);
        RegisterServices(container);
    }

    private static async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
    {
        var version = await packageConfiguration.GetConfigurationVersionAsync().ConfigureAwait(false);

        HeidiConfigV1 config;
        if (version == CurrentConfigVersion.VERSION)
        {
            HeidiConfigV1? result = await packageConfiguration.LoadConfigurationAsync<HeidiConfigV1>().ConfigureAwait(false);
            config = result ?? new HeidiConfigV1();
        }
        else
        {
            config = new HeidiConfigV1();
            await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        }

        // this is temporarly to support the old way of storing the configuration
        if (string.IsNullOrWhiteSpace(config.ConfigPath) && string.IsNullOrWhiteSpace(config.ConfigFilename) && string.IsNullOrWhiteSpace(config.ExecutableFilename))
        {
            container.RegisterSingleton<IHeidiSettings>(() =>
            {
                var oldSettingsProvider = new EnvironmentVariablesHeidiSettings();
                
                var c = new HeidiConfigV1
                {
                    ConfigPath = oldSettingsProvider.ConfigPath,
                    ConfigFilename = oldSettingsProvider.ConfigFilename,
                    ExecutableFilename = oldSettingsProvider.DefaultExe,
                };
                _ = packageConfiguration.PersistConfigurationAsync(c, CurrentConfigVersion.VERSION); // do not await

                return new HeidiModuleConfiguration(c.ConfigPath, c.ConfigFilename, c.ExecutableFilename);
            });
        }
        else
        {
            container.RegisterInstance<IHeidiSettings>(new HeidiModuleConfiguration(config.ConfigPath, config.ConfigFilename, config.ExecutableFilename));
        }
    }
    
    private static void RegisterServices(Container container)
    {
        RegisterPluginHooks(container);
        RegisterInternals(container);
    }

    private static void RegisterPluginHooks(Container container)
    {
        // repository actions
        container.Collection.Append<IActionDeserializer, ActionHeidiDatabasesV1Deserializer>(Lifestyle.Singleton);
        container.Collection.Append<IActionToRepositoryActionMapper, ActionHeidiDatabasesV1Mapper>(Lifestyle.Singleton);

        // ordering
        // (see Statistics for example)

        // action executor
        // (see Statistics for example)

        // variable provider
        container.Collection.Append<IVariableProvider, HeidiDbVariableProvider>(Lifestyle.Singleton);

        // module
        container.Collection.Append<IModule, HeidiModule>(Lifestyle.Singleton);
    }

    private static void RegisterInternals(Container container)
    {
        container.Register<IHeidiConfigurationService, HeidiConfigurationService>(Lifestyle.Singleton);
        container.Register<IHeidiPortableConfigReader, HeidiPortableConfigReader>(Lifestyle.Singleton);
        container.RegisterInstance<IHeidiRepositoryExtractor>(ExtractRepositoryFromHeidi.Instance);
        container.RegisterInstance<IHeidiPasswordDecoder>(HeidiPasswordDecoder.Instance);
    }

    void IPackage.RegisterServices(Container container)
    {
        throw new NotImplementedException();
    }
}