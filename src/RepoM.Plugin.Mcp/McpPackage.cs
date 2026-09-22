namespace RepoM.Plugin.Mcp;

using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin;
using RepoM.Plugin.Mcp.PersistentConfiguration;
using SimpleInjector;

[UsedImplicitly]
public class McpPackage : IPackage
{
    public string Name => "McpPackage"; // do not change this name, it is part of the persistent filename

    public async Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        await ExtractAndRegisterConfiguration(container, packageConfiguration).ConfigureAwait(false);
        RegisterInternals(container);
    }

    private static async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
    {
        var version = await packageConfiguration.GetConfigurationVersionAsync().ConfigureAwait(false);

        McpConfigV1 config;

        if (version == CurrentConfigVersion.VERSION)
        {
            McpConfigV1? result = await packageConfiguration.LoadConfigurationAsync<McpConfigV1>().ConfigureAwait(false);
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

        var port = config.Port ?? 17823;
        if (port is < 1 or > 65535)
        {
            port = 17823;
        }

        var apiKey = config.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            apiKey = McpConfigV1.GenerateApiKey();
        }

        container.RegisterInstance<IMcpConfiguration>(
            new McpConfiguration
            {
                Port = port,
                ApiKey = apiKey,
            });
    }

    private static void RegisterInternals(Container container)
    {
        container.Collection.Append<IModule, McpModule>(Lifestyle.Singleton);
    }

    private static async Task<McpConfigV1> PersistDefaultConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = McpConfigV1.CreateDefault();
        await packageConfiguration.PersistConfigurationAsync(config, McpConfigV1.VERSION).ConfigureAwait(false);
        return config;
    }
}
