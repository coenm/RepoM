namespace RepoM.Plugin.WebBrowser;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.SimpleInjector;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.WebBrowser.PersistentConfiguration;
using RepoM.Plugin.WebBrowser.RepositoryActions;
using RepoM.Plugin.WebBrowser.Services;
using SimpleInjector;

[UsedImplicitly]
public class WebBrowserPackage : IPackage
{
    public string Name => "WebBrowserPackage"; // do not change this name, it is part of the persistant filename

    public async Task RegisterServicesAsync(Container container, IPackageConfiguration packageConfiguration)
    {
        await ExtractAndRegisterConfiguration(container, packageConfiguration).ConfigureAwait(false);
        RegisterPluginHooks(container);
        RegisterInternals(container);
    }

    private static async Task ExtractAndRegisterConfiguration(Container container, IPackageConfiguration packageConfiguration)
    {
        var version = await packageConfiguration.GetConfigurationVersionAsync().ConfigureAwait(false);

        WebBrowserConfigV1 config;

        if (version == CurrentConfigVersion.VERSION)
        {
            WebBrowserConfigV1? result = await packageConfiguration.LoadConfigurationAsync<WebBrowserConfigV1>().ConfigureAwait(false);
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

        container.RegisterInstance(
            new WebBrowserConfiguration
            {
                Browsers = config.Browsers?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>(0),
                Profiles = config.Profiles?.ToDictionary(
                    x => x.Key,
                    x => new BrowserProfileConfig
                        {
                            BrowserName = x.Value.BrowserName,
                            CommandLineArguments = x.Value.CommandLineArguments,
                        }) ?? new Dictionary<string, BrowserProfileConfig>(0),
            });
    }

    private static void RegisterPluginHooks(Container container)
    {
        // repository actions
        container.RegisterActionMenuType<ActionMenu.Model.ActionMenus.Browser.RepositoryActionBrowserV1>();
        container.RegisterActionMenuMapper<ActionMenu.Model.ActionMenus.Browser.RepositoryActionBrowserV1Mapper>(Lifestyle.Singleton);

        // action executor
        container.Register<ICommandExecutor<RepositoryActions.Actions.BrowseRepositoryCommand>, BrowseRepositoryCommandExecutor>(Lifestyle.Singleton);
        container.RegisterDecorator<ICommandExecutor<Core.Plugin.RepositoryActions.Commands.BrowseRepositoryCommand>, CoreBrowseRepositoryCommandExecutorDecorator>(Lifestyle.Singleton);
    }

    private static void RegisterInternals(Container container)
    {
        container.Register<IWebBrowserService, WebBrowserService>(Lifestyle.Singleton);
    }

    /// <remarks>This method is used by reflection to generate documentation file</remarks>
    private static async Task<WebBrowserConfigV1> PersistDefaultConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = WebBrowserConfigV1.CreateDefault();
        await packageConfiguration.PersistConfigurationAsync(config, WebBrowserConfigV1.VERSION).ConfigureAwait(false);
        return config;
    }
    
    /// <remarks>This method is used by reflection to generate documentation file</remarks>
    [UsedImplicitly]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Reflection")]
    private static async Task<WebBrowserConfigV1> PersistExampleConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = WebBrowserConfigV1.CreateExample();
        await packageConfiguration.PersistConfigurationAsync(config, WebBrowserConfigV1.VERSION).ConfigureAwait(false);
        return config;
    }
}