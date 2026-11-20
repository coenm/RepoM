namespace RepoM.Plugin.Demo;

using System.Threading.Tasks;
using RepoM.Api.Plugins;
using RepoM.Core.StartupPlugin;
using SimpleInjector;

/// <summary>Opts into an isolated demo profile and the demo application package.</summary>
public sealed class DemoStartupPlugin : IStartupPlugin
{
    public Task RegisterStartupDependenciesAsync(Container container)
    {
        container.RegisterDecorator<IConfigBasedAppDataPathProviderFactory, DemoPathProviderFactoryDecorator>(Lifestyle.Singleton);
        container.RegisterDecorator<IPluginFinder, DemoPluginFinderDecorator>(Lifestyle.Singleton);
        return Task.CompletedTask;
    }
}
