namespace RepoM.Plugin.Heidi;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Plugin.Heidi.Internal;
using SimpleInjector;
using RepoM.Plugin.Heidi.PersistentConfiguration;
using RepoM.ActionMenu.Interface.Scriban;
using RepoM.Plugin.Heidi.ActionMenu.Context;

[UsedImplicitly]
public class HeidiPackage : IPackage
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

        HeidiConfigV1? config = null;
        if (version == CurrentConfigVersion.VERSION)
        {
            config = await packageConfiguration.LoadConfigurationAsync<HeidiConfigV1>().ConfigureAwait(false);
        }

        config ??= await PersistDefaultConfigAsync(packageConfiguration).ConfigureAwait(false);

        container.RegisterInstance<IHeidiSettings>(new HeidiModuleConfiguration(config.ConfigPath, config.ConfigFilename, config.ExecutableFilename));
    }

    private static void RegisterServices(Container container)
    {
        RegisterPluginHooks(container);
        RegisterInternals(container);
    }

    private static void RegisterPluginHooks(Container container)
    {
        // repository actions

        // ordering
        // (see Statistics for example)

        // action executor
        // (see Statistics for example)

        // module
        container.Collection.Append<IModule, HeidiModule>(Lifestyle.Singleton);

        container.Collection.Append<ITemplateContextRegistration, HeidiDbVariables>(Lifestyle.Singleton);
    }

    private static void RegisterInternals(Container container)
    {
        container.Register<IHeidiConfigurationService, HeidiConfigurationService>(Lifestyle.Singleton);
        container.Register<IHeidiPortableConfigReader, HeidiPortableConfigReader>(Lifestyle.Singleton);
        container.RegisterInstance<IHeidiRepositoryExtractor>(ExtractRepositoryFromHeidi.Instance);
        container.RegisterInstance<IHeidiPasswordDecoder>(HeidiPasswordDecoder.Instance);
    }

    /// <remarks>This method is used by reflection to generate documentation file</remarks>
    private static async Task<HeidiConfigV1> PersistDefaultConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = HeidiConfigV1.CreateDefault();
        await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        return config;
    }
}