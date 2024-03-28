namespace RepoM.App;

using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;
using RepoM.Api.Git.ProcessExecution;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO;
using RepoM.Api.Ordering.Az;
using RepoM.Api.Ordering.Composition;
using RepoM.Api.Ordering.IsPinned;
using RepoM.Api.Ordering.Label;
using RepoM.Api.Ordering.Score;
using RepoM.Api.Ordering.Sum;
using RepoM.Api.RepositoryActions.Decorators;
using RepoM.App.i18n;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryOrdering;
using RepoM.App.Services;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFinder;
using RepoM.Core.Plugin.RepositoryOrdering;
using System.IO.Abstractions;
using System;
using System.Threading.Tasks;
using SimpleInjector;
using Microsoft.Extensions.Logging;
using RepoM.Api.Plugins;
using RepoM.App.Plugins;
using RepoM.App.Services.HotKey;
using RepoM.Api;
using System.Runtime.Caching;
using RepoM.App.ActionMenuCore;

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

        Container.Register<IRepositoryTagsFactory, RepositoryTagsFactoryV2>(Lifestyle.Singleton);
        Container.RegisterDecorator<IRepositoryTagsFactory, LoggingRepositoryTagsFactoryDecorator>(Lifestyle.Singleton);

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

        ActionMenu.Core.Bootstrapper.RegisterServices(Container);
        
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
        Container.Register(typeof(ICommandExecutor<>), new[] { typeof(CoreBootstrapper).Assembly, }, Lifestyle.Singleton);
        Container.RegisterDecorator(
            typeof(ICommandExecutor<>),
            typeof(LoggerCommandExecutorDecorator<>),
            Lifestyle.Singleton);

        Container.RegisterSingleton<HotKeyService>();
        Container.RegisterSingleton<WindowSizeService>();
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