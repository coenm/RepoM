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
using RepoM.Core.Plugin.RepositoryOrdering;
using System.IO.Abstractions;
using System.Reflection;
using System;
using System.Threading.Tasks;
using SimpleInjector;
using Microsoft.Extensions.Logging;
using RepoM.Api.Plugins;
using RepoM.App.Plugins;
using RepoM.App.Services.HotKey;
using RepoM.Api;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.Common;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using System.Runtime.Caching;

internal static class Bootstrapper
{
    public static readonly Container Container = new();

    public static void RegisterServices(IFileSystem fileSystem)
    {
        Container.RegisterInstance<ObjectCache>(MemoryCache.Default);
        Container.Register<MainWindow>(Lifestyle.Singleton);
        Container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>(Lifestyle.Singleton);
        Container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>(Lifestyle.Singleton);
        Container.Register<IRepositoryDetectorFactory, DefaultRepositoryDetectorFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>(Lifestyle.Singleton);
        Container.Register<IGitRepositoryFinderFactory, GitRepositoryFinderFactory>(Lifestyle.Singleton);
        Container.RegisterInstance<IAppDataPathProvider>(DefaultAppDataPathProvider.Instance);
        Container.Register<IRepositoryActionProvider, DefaultRepositoryActionProvider>(Lifestyle.Singleton);
        Container.RegisterDecorator<IRepositoryActionProvider, LoggingRepositoryActionProviderDecorator>(Lifestyle.Singleton);
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

        RegisterExpressionStringVariableProviders();

        Container.Collection.Append<Core.Plugin.VariableProviders.IVariableProvider, CustomEnvironmentVariableVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<Core.Plugin.VariableProviders.IVariableProvider, RepoMVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<Core.Plugin.VariableProviders.IVariableProvider, RepositoryVariableProvider>(Lifestyle.Singleton);
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

        // Register custom Repository Action deserializers
        var actionDeserializerTypes = GetExportedTypesFrom(typeof(IActionDeserializer).Assembly)
          .Where(t => typeof(IActionDeserializer).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
          .Where(t => t.GetTypeInfo() is { IsAbstract: false, IsGenericTypeDefinition: false, })
          .Where(t => t != typeof(DefaultActionDeserializer<>));
        Container.Collection.Register<IActionDeserializer>(actionDeserializerTypes, Lifestyle.Singleton);

        // Register all repository action types
        GetExportedTypesFrom(typeof(IActionDeserializer).Assembly)
            .Where(t => typeof(Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction).GetTypeInfo().IsAssignableFrom(t.GetTypeInfo()))
            .Where(t => t.GetTypeInfo() is { IsAbstract: false, IsGenericTypeDefinition: false, })
            .Where(t => t != typeof(Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction))
            .ForEach(t => Container.RegisterDefaultRepositoryActionDeserializerForType(t));

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
                return Enumerable.Empty<Type>();
            }
        }
        
        Container.Register<ActionMapperComposition>(Lifestyle.Singleton);
        Container.Collection.Register<IActionToRepositoryActionMapper>(
            new[] { typeof(IActionToRepositoryActionMapper).Assembly, },
            Lifestyle.Singleton);

        Container.Register<IRepositoryActionDeserializer, YamlDynamicRepositoryActionDeserializer>(Lifestyle.Singleton);
        Container.Register<RepositorySpecificConfiguration>(Lifestyle.Singleton);


        Container.RegisterSingleton<IRepositoryComparerFactory, RepositoryComparerCompositionFactory>();
        Container.RegisterSingleton<IRepositoryScoreCalculatorFactory, RepositoryScoreCalculatorFactory>();

        CoreBootstrapper.RegisterRepositoryComparerConfigurationsTypes(Container);
        CoreBootstrapper.RegisterRepositoryScorerConfigurationsTypes(Container);
        
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

    private static void RegisterExpressionStringVariableProviders()
    {
        Container.Collection.Append<IVariableProvider, DateTimeNowVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, DateTimeTimeVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, DateTimeDateVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, EmptyVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, SlashVariableProvider>(Lifestyle.Singleton);
        Container.Collection.Append<IVariableProvider, BackslashVariableProvider>(Lifestyle.Singleton);
    }

    public static async Task RegisterPlugins(
        IPluginFinder pluginFinder,
        IFileSystem fileSystem,
        ILoggerFactory loggerFactory)
    {
        Container.Register<ModuleService>(Lifestyle.Singleton);
        Container.RegisterInstance(pluginFinder);

        var coreBootstrapper = new CoreBootstrapper(pluginFinder, fileSystem, DefaultAppDataPathProvider.Instance, loggerFactory);
        var baseDirectory = fileSystem.Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        await coreBootstrapper.LoadAndRegisterPluginsAsync(Container, baseDirectory).ConfigureAwait(false);
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