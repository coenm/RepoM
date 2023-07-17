namespace RepoM.App;

using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.VariableProviders.DateTime;
using ExpressionStringEvaluator.VariableProviders;
using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;
using RepoM.Api.Git.ProcessExecution;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.VariableProviders;
using RepoM.Api.IO;
using RepoM.Api.Ordering.Az;
using RepoM.Api.Ordering.Composition;
using RepoM.Api.Ordering.IsPinned;
using RepoM.Api.Ordering.Label;
using RepoM.Api.Ordering.Score;
using RepoM.Api.Ordering.Sum;
using RepoM.Api.RepositoryActions.Decorators;
using RepoM.Api.RepositoryActions.Executors;
using RepoM.App.i18n;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryOrdering;
using RepoM.App.Services;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFinder;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleInjector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Plugins;
using RepoM.App.Plugins;
using RepoM.App.Services.HotKey;
using Newtonsoft.Json;
using RepoM.Core.Plugin;

internal static class Bootstrapper
{
    public static readonly Container Container = new();

    public static void RegisterServices(IFileSystem fileSystem)
    {
        Container.Register<MainWindow>(Lifestyle.Singleton);
        Container.RegisterInstance(StatusCharacterMap.Instance);
        Container.Register<StatusCompressor>(Lifestyle.Singleton);
        Container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>(Lifestyle.Singleton);
        Container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>(Lifestyle.Singleton);
        Container.Register<IRepositoryDetectorFactory, DefaultRepositoryDetectorFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>(Lifestyle.Singleton);
        Container.Register<IGitRepositoryFinderFactory, GitRepositoryFinderFactory>(Lifestyle.Singleton);
        Container.RegisterInstance<IAppDataPathProvider>(DefaultAppDataPathProvider.Instance);
        Container.Register<IRepositoryActionProvider, DefaultRepositoryActionProvider>(Lifestyle.Singleton);
        Container.Register<IRepositoryReader, DefaultRepositoryReader>(Lifestyle.Singleton);
        Container.Register<IRepositoryWriter, DefaultRepositoryWriter>(Lifestyle.Singleton);
        Container.Register<IRepositoryStore, DefaultRepositoryStore>(Lifestyle.Singleton);
        Container.Register<IPathProvider, DefaultDriveEnumerator>(Lifestyle.Singleton);
        Container.Register<IPathSkipper, WindowsPathSkipper>(Lifestyle.Singleton);
        Container.Register<IThreadDispatcher, WpfThreadDispatcher>(Lifestyle.Singleton);
        Container.Register<IGitCommander, ProcessExecutingGitCommander>(Lifestyle.Singleton);
        Container.Register<IAppSettingsService, FileAppSettingsService>(Lifestyle.Singleton);
        Container.Register<ICompareSettingsService, FilesCompareSettingsService>(Lifestyle.Singleton);
        Container.Register<IFilterSettingsService, FilesFilterSettingsService>(Lifestyle.Singleton);
        Container.Register<IAutoFetchHandler, DefaultAutoFetchHandler>(Lifestyle.Singleton);
        Container.Register<IRepositoryIgnoreStore, DefaultRepositoryIgnoreStore>(Lifestyle.Singleton);
        Container.Register<ITranslationService, ResourceDictionaryTranslationService>(Lifestyle.Singleton);
        Container.RegisterInstance<IClock>(SystemClock.Instance);
        Container.Register<IRepositoryTagsFactory, RepositoryTagsConfigurationFactory>(Lifestyle.Singleton);
        Container.Register<RepositoryConfigurationReader>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerManager, RepositoryComparerManager>(Lifestyle.Singleton);

        Container.Register<IRepositoryMatcher, RepositoryMatcher>(Lifestyle.Singleton);
        Container.Register<IRepositoryFilteringManager, RepositoryFilteringManager>(Lifestyle.Singleton);
        Container.Collection.Append<INamedQueryParser, DefaultQueryParser>(Lifestyle.Singleton);

        Container.Collection.Append<IQueryMatcher, IsPinnedMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher, TagMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher, HasUnPushedChangesMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher>(() => new FreeTextMatcher(ignoreCase: true, ignoreCaseTag: true), Lifestyle.Singleton);

        Container.Register<IModuleManager, ModuleManager>(Lifestyle.Singleton);


        Container.Collection.Append<ISingleGitRepositoryFinderFactory, GravellGitRepositoryFinderFactory>(Lifestyle.Singleton);

        Container.RegisterInstance<IFileSystem>(fileSystem);

        Container.Register<IRepositoryExpressionEvaluator, RepositoryExpressionEvaluator>(Lifestyle.Singleton);
        Assembly[] repoExpressionEvaluators =
            {
                typeof(IVariableProvider).Assembly,
                typeof(RepositoryExpressionEvaluator).Assembly,
                typeof(IRepositoryExpressionEvaluator).Assembly,
            };

        Container.Collection.Append<IVariableProvider, DateTimeNowVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, DateTimeTimeVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, DateTimeDateVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, EmptyVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, CustomEnvironmentVariableVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, RepoMVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, RepositoryVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, SlashVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, BackslashVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, VariableProviderAdapter>(Lifestyle.Singleton);

        Container.Collection.Register(typeof(IMethod), repoExpressionEvaluators, Lifestyle.Singleton);
        Container.RegisterInstance(new DateTimeVariableProviderOptions
        {
            DateTimeProvider = () => DateTime.Now,
        });
        Container.RegisterInstance(new DateTimeNowVariableProviderOptions
        {
            DateTimeProvider = () => DateTime.Now,
        });
        Container.RegisterInstance(new DateTimeDateVariableProviderOptions
        {
            DateTimeProvider = () => DateTime.Now,
        });

        Container.Register<ActionDeserializerComposition>(Lifestyle.Singleton);
        Container.Collection.Register<IActionDeserializer>(
            new[] { typeof(IActionDeserializer).Assembly, },
            Lifestyle.Singleton);

        Container.Register<ActionMapperComposition>(Lifestyle.Singleton);
        Container.Collection.Register<IActionToRepositoryActionMapper>(
            new[] { typeof(IActionToRepositoryActionMapper).Assembly, },
            Lifestyle.Singleton);

        Container.Register<JsonDynamicRepositoryActionDeserializer>(Lifestyle.Singleton);
        Container.Register<YamlDynamicRepositoryActionDeserializer>(Lifestyle.Singleton);
        Container.Register<RepositorySpecificConfiguration>(Lifestyle.Singleton);


        Container.RegisterSingleton<IRepositoryComparerFactory, RepositoryComparerCompositionFactory>();
        Container.RegisterSingleton<IRepositoryScoreCalculatorFactory, RepositoryScoreCalculatorFactory>();

        Container.Collection.Register(
            typeof(IConfigurationRegistration),
            new[] { typeof(IConfigurationRegistration).Assembly, typeof(IsPinnedScorerConfigurationV1Registration).Assembly, },
            Lifestyle.Singleton);

        Container.Register<IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1>, IsPinnedScorerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryScoreCalculatorFactory<TagScorerConfigurationV1>, TagScorerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerFactory<AlphabetComparerConfigurationV1>, AzRepositoryComparerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerFactory<CompositionComparerConfigurationV1>, CompositionRepositoryComparerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerFactory<ScoreComparerConfigurationV1>, ScoreRepositoryComparerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerFactory<SumComparerConfigurationV1>, SumRepositoryComparerFactory>(Lifestyle.Singleton);

        Container.RegisterSingleton<ActionExecutor>();
        Container.Register(typeof(IActionExecutor<>), new[] { typeof(BrowseActionExecutor).Assembly, }, Lifestyle.Singleton);
        Container.RegisterDecorator(
            typeof(IActionExecutor<>),
            typeof(LoggerActionExecutorDecorator<>),
            Lifestyle.Singleton);

        Container.RegisterSingleton<HotKeyService>();
        Container.RegisterSingleton<WindowSizeService>();
    }

    public static async Task RegisterPlugins(IPluginFinder pluginFinder, IFileSystem fileSystem)
    {
        Container.Register<ModuleService>(Lifestyle.Singleton);
        Container.RegisterInstance(pluginFinder);

        var baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        IEnumerable<PluginInfo> pluginInformation =  pluginFinder.FindPlugins(baseDirectory).ToArray();

        static PluginSettings Convert(PluginInfo pluginInfo, string baseDir, bool enabled)
        {
            return new PluginSettings(pluginInfo.Name, pluginInfo.AssemblyPath.Replace(baseDir, string.Empty), enabled);
        }

        var appSettingsService = new FileAppSettingsService(DefaultAppDataPathProvider.Instance, fileSystem, NullLogger.Instance);

        if (appSettingsService.Plugins.Count == 0)
        {
            appSettingsService.Plugins = pluginInformation.Select(plugin => Convert(plugin, baseDirectory, true)).ToList();
        }
        else
        {
            IEnumerable<PluginSettings> newFoundPlugins = pluginInformation
                .Where(pluginInfo => appSettingsService.Plugins.TrueForAll(plugin => plugin.Name != pluginInfo.Name))
                .Select(plugin => Convert(plugin, baseDirectory, false));

            var pluginsListCopy = appSettingsService.Plugins.ToList();
            pluginsListCopy.AddRange(newFoundPlugins);
            appSettingsService.Plugins = pluginsListCopy;
        }

        IEnumerable<string> enabledPlugins = appSettingsService.Plugins.Where(x => x.Enabled).Select(xxx => xxx.Name);
        
        Assembly[] assemblies = pluginInformation
            .Where(plugin => enabledPlugins.Contains(plugin.Name))
            .Select(plugin => Assembly.Load(AssemblyName.GetAssemblyName(plugin.AssemblyPath)))
            .ToArray();

        if (assemblies.Any())
        {
            await Container.RegisterPackagesAsync(
                assemblies,
                filename => new FileBasedPackageConfiguration(DefaultAppDataPathProvider.Instance, fileSystem, filename)).ConfigureAwait(false); 
        }
    }

    public static void RegisterLogging(ILoggerFactory loggerFactory)
    {
        // https://stackoverflow.com/questions/41243485/simple-injector-register-iloggert-by-using-iloggerfactory-createloggert

        Container.RegisterInstance<ILoggerFactory>(loggerFactory);
        Container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));

        Container.RegisterConditional(
            typeof(ILogger),
            c => c.Consumer == null
                ? typeof(Logger<object>)
                : typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
            Lifestyle.Singleton,
            _ => true);
    }
}

file class FileBasedPackageConfiguration : IPackageConfiguration
{
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IFileSystem _fileSystem;
    private readonly string _filename;

    public FileBasedPackageConfiguration(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem, string filename)
    {
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _filename = filename ?? throw new ArgumentNullException(nameof(filename));
    }

    public async Task<int?> GetConfigurationVersionAsync()
    {
        ConfigEnvelope<object>? result = await LoadAsync<object>().ConfigureAwait(false);
        return result?.Version;
    }

    public async Task<T?> LoadConfigurationAsync<T>() where T : class, new()
    {
        ConfigEnvelope<T>? result = await LoadAsync<T>().ConfigureAwait(false);
        return result?.Payload;
    }

    public async Task PersistConfigurationAsync<T>(T configuration, int version)
    {
        if (configuration == null)
        {
            return;
        }

        var filename = GetFilename();

        var json = JsonConvert.SerializeObject(new ConfigEnvelope<T> { Version = version, Payload = configuration, }, Formatting.Indented);
        await _fileSystem.File.WriteAllTextAsync(filename, json).ConfigureAwait(false);
    }

    private string GetFilename()
    {
        return Path.Combine(_appDataPathProvider.GetAppDataPath(), "Module", _filename + ".json");
    }

    private async Task<ConfigEnvelope<T>?> LoadAsync<T>()
    {
        var filename = GetFilename();
        if (!_fileSystem.File.Exists(filename))
        {
            return null;
        }

        var json = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
        ConfigEnvelope<T>? result = JsonConvert.DeserializeObject<ConfigEnvelope<T>>(json);
        return result;
    }

    private sealed class ConfigEnvelope<T>
    {
        public int Version { get; init; }

        public T? Payload { get; init; }
    }
}
