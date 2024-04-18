namespace RepoM.Api;

using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.IO;
using RepoM.Api.Plugins;
using SimpleInjector;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System;
using RepoM.Api.Plugins.SimpleInjector;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class CoreBootstrapper
{
    private readonly IPluginFinder _pluginFinder;
    private readonly IFileSystem _fileSystem;
    private readonly IAppDataPathProvider _appDataProvider;
    private readonly ILoggerFactory _loggerFactory;

    private static readonly Assembly _thisAssembly = typeof(CoreBootstrapper).Assembly;

    public CoreBootstrapper(IPluginFinder pluginFinder, IFileSystem fileSystem, IAppDataPathProvider appDataProvider, ILoggerFactory loggerFactory)
    {
        _pluginFinder = pluginFinder ?? throw new ArgumentNullException(nameof(pluginFinder));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _appDataProvider = appDataProvider ?? throw new ArgumentNullException(nameof(appDataProvider));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public static void RegisterRepositoryScorerConfigurationsTypes(Container container)
    {
        foreach (Type type in GetNonAbstractNonGenericInheritedExportedTypesFrom<IRepositoryScorerConfiguration>(_thisAssembly))
        {
            container.RegisterScorerConfigurationForType(type);
        }
    }

    public static void RegisterRepositoryComparerConfigurationsTypes(Container container)
    {
        foreach (Type type in GetNonAbstractNonGenericInheritedExportedTypesFrom<IRepositoriesComparerConfiguration>(_thisAssembly))
        {
            container.RegisterComparerConfigurationForType(type);
        }
    }

    public async Task LoadAndRegisterPluginsAsync(Container container, string baseDirectory)
    {
        _ = container ?? throw new ArgumentNullException(nameof(container));

        IEnumerable<PluginInfo> pluginInformation = _pluginFinder.FindPlugins(baseDirectory).ToArray();

        var appSettingsService = new FileAppSettingsService(_appDataProvider, _fileSystem, _loggerFactory.CreateLogger<FileAppSettingsService>());

        if (appSettingsService.Plugins.Count == 0)
        {
            appSettingsService.Plugins = pluginInformation.Select(plugin => ConvertToPluginSettings(plugin, baseDirectory, true)).ToList();
        }
        else
        {
            IEnumerable<PluginSettings> newFoundPlugins = pluginInformation
                .Where(pluginInfo => appSettingsService.Plugins.TrueForAll(plugin => plugin.Name != pluginInfo.Name))
                .Select(plugin => ConvertToPluginSettings(plugin, baseDirectory, false));

            var pluginsListCopy = appSettingsService.Plugins.ToList();
            pluginsListCopy.AddRange(newFoundPlugins);
            appSettingsService.Plugins = pluginsListCopy;
        }

        IEnumerable<string> enabledPlugins = appSettingsService.Plugins
            .Where(plugin => plugin.Enabled)
            .Select(plugin => plugin.Name);

        Assembly[] assemblies = pluginInformation
            .Where(plugin => enabledPlugins.Contains(plugin.Name))
            .Select(plugin => Assembly.Load(AssemblyName.GetAssemblyName(plugin.AssemblyPath)))
            .ToArray();

        if (assemblies.Length > 0)
        {
            await container
                .RegisterPackagesAsync(
                    assemblies,
                    filename => new FileBasedPackageConfiguration(
                        DefaultAppDataPathProvider.Instance,
                        _fileSystem,
                        _loggerFactory.CreateLogger<FileBasedPackageConfiguration>(),
                        filename))
                .ConfigureAwait(false);
        }
    }

    private static PluginSettings ConvertToPluginSettings(PluginInfo pluginInfo, string baseDir, bool enabled)
    {
        return new PluginSettings(pluginInfo.Name, pluginInfo.AssemblyPath.Replace(baseDir, string.Empty), enabled);
    }

    static IEnumerable<Type> GetExportedTypesFrom(Assembly assembly)
    {
        try
        {
            return assembly.DefinedTypes.Select(info => info.AsType());
        }
        catch (NotSupportedException)
        {
            // A type load exception would typically happen on an Anonymously Hosted DynamicMethods
            // Assembly and it would be safe to skip this exception.
            return [];
        }
    }

    static IEnumerable<Type> GetNonAbstractNonGenericInheritedExportedTypesFrom<T>(Assembly assembly)
    {
        return GetExportedTypesFrom(assembly)
               .Where(t => typeof(T).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
               .Where(t => t.GetTypeInfo() is { IsAbstract: false, IsGenericTypeDefinition: false, });

    }
}