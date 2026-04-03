namespace RepoM.App;

using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;
using RepoM.Api.Git.ProcessExecution;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO;
using RepoM.Api.Ordering.Az;
using RepoM.Api.Ordering.Composition;
using RepoM.Api.Ordering.IsFavorite;
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
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryFinder;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Favorite;
using RepoM.Core.Repositories.Reading;
using RepoM.Core.Repositories.Scanning;
using RepoM.Core.Repositories.Store;
using RepoM.Core.Repositories.Watching;
using System.IO.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleInjector;
using Microsoft.Extensions.Logging;
using RepoM.Api.Plugins;
using RepoM.App.Plugins;
using RepoM.App.Services.HotKey;
using RepoM.Api;
using System.Runtime.Caching;
using System.Windows;
using RepoM.App.ActionMenuCore;

internal static class Bootstrapper
{
    public static readonly Container Container = new();

    public static void RegisterServices(IFileSystem fileSystem, IAppDataPathProvider appDataProvider)
    {
        Container.RegisterInstance<ObjectCache>(MemoryCache.Default);
        Container.RegisterSingleton<Window>(() => Container.GetInstance<MainWindow>());
        Container.Register<MainWindow>(Lifestyle.Singleton);

        // New repository infrastructure
        Container.Register<IRepositoryStore, RepositoryStore>(Lifestyle.Singleton);
        Container.RegisterInstance(new GitRepositoryScannerSettings(Math.Max(1, Environment.ProcessorCount / 2)));
        Container.Register<IRepositoryScanner, GitRepositoryScanner>(Lifestyle.Singleton);
        Container.Register<IRepositoryWatcher, FileSystemRepositoryWatcher>(Lifestyle.Singleton);
        Container.Register<IRepositoryInfoReader, LibGit2SharpRepositoryInfoReader>(Lifestyle.Singleton);
        Container.Register<IFavoriteService>(() => Container.GetInstance<FavoriteService>(), Lifestyle.Singleton);
        Container.Register<FavoriteService>(() => new FavoriteService(Container.GetInstance<IFavoriteStore>()), Lifestyle.Singleton);
        Container.Register<IFavoriteStore, RepoM.Api.Favorite.FavoriteYamlStore>(Lifestyle.Singleton);
        Container.Register<RepositoryMonitoringStateService>(Lifestyle.Singleton);
        Container.Register<IRepositoryMonitoringService>(() => Container.GetInstance<RepositoryMonitoringStateService>(), Lifestyle.Singleton);
        Container.Register<IRepositoryMonitoringEvents>(() => Container.GetInstance<RepositoryMonitoringStateService>(), Lifestyle.Singleton);
        Container.Register<RepoM.Core.Repositories.RepositoryMonitorService>(Lifestyle.Singleton);

        // Register RepositoryMonitorService as IModule
        Container.Collection.Append<IModule, RepoM.Core.Repositories.RepositoryMonitorService>(Lifestyle.Singleton);

        Container.RegisterInstance(appDataProvider);
        Container.Register<IRepositoryWriter, DefaultRepositoryWriter>(Lifestyle.Singleton);
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

        Container.Register<IUserMenuActionMenuFactory, UserMenuActionMenuFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryTagsFactory, RepositoryTagsFactoryV2>(Lifestyle.Singleton);

        Container.Register<IRepositoryComparerManager, RepositoryComparerManager>(Lifestyle.Singleton);

        Container.Register<IRepositoryMatcher, RepositoryMatcher>(Lifestyle.Singleton);
        Container.Register<IRepositoryFilteringManager, RepositoryFilteringManager>(Lifestyle.Singleton);
        CoreBootstrapper.RegisterQuickFilterServices(Container);
        Container.Collection.Append<INamedQueryParser, DefaultQueryParser>(Lifestyle.Singleton);

        Container.Collection.Append<IQueryMatcher, IsFavoriteMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher, IsMonitoredMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher, IsBehindMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher, IsBareRepositoryMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher, TagMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher, NameMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher, HasUnPushedChangesMatcher>(Lifestyle.Singleton);
        Container.Collection.Append<IQueryMatcher>(() => new FreeTextMatcher(ignoreCase: true, ignoreCaseTag: true), Lifestyle.Singleton);

        Container.Register<IModuleManager, ModuleManager>(Lifestyle.Singleton);

        Container.RegisterInstance<IFileSystem>(fileSystem);

        // Register path provider function for RepositoryMonitorService
        Container.RegisterInstance<Func<IEnumerable<string>>>(() => Container.GetInstance<IPathProvider>().GetPaths());

        ActionMenu.Core.Bootstrapper.RegisterServices(Container);

        Container.RegisterSingleton<IRepositoryComparerFactory, RepositoryComparerCompositionFactory>();
        Container.RegisterSingleton<IRepositoryScoreCalculatorFactory, RepositoryScoreCalculatorFactory>();

        CoreBootstrapper.RegisterRepositoryComparerConfigurationsTypes(Container);
        CoreBootstrapper.RegisterRepositoryScorerConfigurationsTypes(Container);

        Container.Register<IRepositoryScoreCalculatorFactory<IsFavoriteScorerConfigurationV1>, IsFavoriteScorerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryScoreCalculatorFactory<TagScorerConfigurationV1>, TagScorerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerFactory<AlphabetComparerConfigurationV1>, AzRepositoryComparerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerFactory<CompositionComparerConfigurationV1>, CompositionRepositoryComparerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerFactory<ScoreComparerConfigurationV1>, ScoreRepositoryComparerFactory>(Lifestyle.Singleton);
        Container.Register<IRepositoryComparerFactory<SumComparerConfigurationV1>, SumRepositoryComparerFactory>(Lifestyle.Singleton);

        Container.RegisterSingleton<ActionExecutor>();
        Container.Register(typeof(ICommandExecutor<>), [typeof(CoreBootstrapper).Assembly,], Lifestyle.Singleton);
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
        ILoggerFactory loggerFactory,
        IAppDataPathProvider appDataPathProvider)
    {
        Container.Register<ModuleService>(Lifestyle.Singleton);
        Container.RegisterInstance(pluginFinder);

        var coreBootstrapper = new CoreBootstrapper(pluginFinder, fileSystem, appDataPathProvider, loggerFactory);
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
