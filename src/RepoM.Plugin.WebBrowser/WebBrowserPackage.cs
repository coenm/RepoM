namespace RepoM.Plugin.WebBrowser;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.WebBrowser.ActionProvider;
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
        // new style
        container.RegisterActionMenuType<ActionMenu.Model.ActionMenus.Browser.RepositoryActionBrowserV1>();
        container.RegisterActionMenuMapper<ActionMenu.Model.ActionMenus.Browser.RepositoryActionBrowserV1Mapper>(Lifestyle.Singleton);

        // old style
        container.RegisterDefaultRepositoryActionDeserializerForType<RepositoryActionBrowserV1>();
        container.Collection.Append<IActionToRepositoryActionMapper, ActionBrowserV1Mapper>(Lifestyle.Singleton);

        // action executor
        container.Register(typeof(ICommandExecutor<>), new[] { typeof(WebBrowserPackage).Assembly, }, Lifestyle.Singleton);
        container.RegisterDecorator<ICommandExecutor<Core.Plugin.RepositoryActions.Commands.BrowseRepositoryCommand>, CoreBrowseRepositoryCommandExecutorDecorator>(Lifestyle.Singleton);
    }

    private static void RegisterInternals(Container container)
    {
        container.Register<IWebBrowserService, WebBrowserService>(Lifestyle.Singleton);
    }

    /// <remarks>This method is used by reflection to generate documentation file</remarks>>
    private static async Task<WebBrowserConfigV1> PersistDefaultConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = new WebBrowserConfigV1
        {
            Browsers = null,
            Profiles = null,
        };

        await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        return config;
    }


    /// <remarks>This method is used by reflection to generate documentation file</remarks>>
    [UsedImplicitly]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Reflection")]
    private static async Task<WebBrowserConfigV1> PersistExampleConfigAsync(IPackageConfiguration packageConfiguration)
    {
        var config = new WebBrowserConfigV1
            {
                Browsers = new Dictionary<string, string>
                    {
                        { "Edge", "C:\\PathTo\\msedge.exe" },
                        { "FireFox", "C:\\PathTo\\Mozilla\\firefox.exe" },
                    },
                Profiles = new Dictionary<string, ProfileConfig>
                    {
                        { "Work", new ProfileConfig { BrowserName = "Edge", CommandLineArguments = "\"--profile-directory=Profile 4\" {url}", } },
                        { "Incognito", new ProfileConfig { BrowserName = "Edge", CommandLineArguments = "-inprivate", } },
                        { "Incognito2", new ProfileConfig { BrowserName = "FireFox", CommandLineArguments = "-inprivate {url}", } },
                    },
            };

        await packageConfiguration.PersistConfigurationAsync(config, CurrentConfigVersion.VERSION).ConfigureAwait(false);
        return config;
    }
}