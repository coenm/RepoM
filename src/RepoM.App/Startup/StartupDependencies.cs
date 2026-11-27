namespace RepoM.App.Startup;

using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using RepoM.Api;
using RepoM.Api.Plugins;
using RepoM.Core.StartupPlugin;
using SimpleInjector;

internal sealed class StartupDependencies : IDisposable
{
    private readonly Container _container = new();

    private StartupDependencies()
    {
    }

    public IFileSystem FileSystem => _container.GetInstance<IFileSystem>();
    public IHmacService HmacService => _container.GetInstance<IHmacService>();
    public IPluginFinder PluginFinder => _container.GetInstance<IPluginFinder>();
    public IConfigBasedAppDataPathProviderFactory PathProviderFactory =>
        _container.GetInstance<IConfigBasedAppDataPathProviderFactory>();

    public static async Task<StartupDependencies> CreateAsync(StartupArguments arguments)
    {
        var dependencies = new StartupDependencies();
        try
        {
            await CoreBootstrapper.RegisterStartupDependenciesAsync(
                dependencies._container, arguments.ConfigurationArguments).ConfigureAwait(false);

            if (arguments.PluginPath != null)
            {
                IStartupPlugin plugin = StartupPluginLoader.Load(arguments.PluginPath);
                dependencies._container.Options.AllowOverridingRegistrations = true;
                await plugin.RegisterStartupDependenciesAsync(dependencies._container).ConfigureAwait(false);
                dependencies._container.Options.AllowOverridingRegistrations = false;
            }

            dependencies._container.Verify();
            return dependencies;
        }
        catch
        {
            dependencies.Dispose();
            throw;
        }
    }

    public void RegisterInstances(Container applicationContainer)
    {
        // RegisterInstance does not transfer disposal ownership to the application container.
        applicationContainer.RegisterInstance(FileSystem);
        applicationContainer.RegisterInstance(HmacService);
        applicationContainer.RegisterInstance(PluginFinder);
        applicationContainer.RegisterInstance(PathProviderFactory);
    }

    public void Dispose() => _container.Dispose();
}
