[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.VariableProviders;
using ExpressionStringEvaluator.VariableProviders.DateTime;
using Hardcodet.Wpf.TaskbarNotification;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.Git.AutoFetch;
using RepoM.Api.Git.ProcessExecution;
using RepoM.Api.IO;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.App.i18n;
using RepoM.Core.Plugin;
using SimpleInjector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using RepoM.App.Services;
using RepoM.Api.IO.VariableProviders;
using RepoM.Api.Ordering.IsPinned;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Api.Ordering.Az;
using RepoM.Api.Ordering.Composition;
using RepoM.Api.Ordering.Label;
using RepoM.Api.Ordering.Score;
using RepoM.Api.Ordering.Sum;
using Container = SimpleInjector.Container;
using RepoM.Api.RepositoryActions.Decorators;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Api.RepositoryActions.Executors;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryOrdering;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryFinder;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryFiltering.QueryMatchers;
using RepoM.Core.Plugin.RepositoryFiltering;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static Timer? _updateTimer;
    private HotKey? _hotkey;
    private static IRepositoryMonitor? _repositoryMonitor;
    private TaskbarIcon? _notifyIcon;
    private List<IModule>? _modules;
    private IAppSettingsService? _settings;
    private static readonly Container _container = new();

    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure the current culture passed into bindings is the OS culture.
        // By default, WPF uses en-US as the culture, regardless of the system settings.
        // see: https://stackoverflow.com/a/520334/704281
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

        Application.Current.Resources.MergedDictionaries[0] = ResourceDictionaryTranslationService.ResourceDictionary;
        _notifyIcon = FindResource("NotifyIcon") as TaskbarIcon;

        var fileSystem = new FileSystem();

        IConfiguration config = SetupConfiguration(fileSystem);
        ILoggerFactory loggerFactory = CreateLoggerFactory(config);
        ILogger logger = loggerFactory.CreateLogger(nameof(App));
        logger.LogInformation("Started");
        RegisterLogging(loggerFactory);
        RegisterServices(_container, fileSystem);

#if DEBUG
        _container.Verify(VerificationOption.VerifyAndDiagnose);
#endif

        UseRepositoryMonitor(_container);

        _updateTimer = new Timer(async _ => await CheckForUpdatesAsync(), null, 5000, Timeout.Infinite);

        // We noticed that the hotkey registration causes a high CPU utilization if the window was not shown before.
        // To fix this, we need to make the window visible in EnsureWindowHandle() but we set the opacity to 0.0 to prevent flickering
        MainWindow window = _container.GetInstance<MainWindow>();
        EnsureWindowHandle(window);

        _hotkey = new HotKey(47110815);
        _hotkey.Register(window, HotKey.VK_R, HotKey.MOD_ALT | HotKey.MOD_CTRL, OnHotKeyPressed);

        _modules = _container.GetAllInstances<IModule>().ToList();
        StartModules(_modules);

        _settings = _container.GetInstance<IAppSettingsService>();

        if (_settings.MenuWidth > 0)
        {
            window.Width = _settings.MenuWidth;
        }

        if (_settings.MenuHeight > 0)
        {
            window.Height = _settings.MenuHeight;
        }

        window.SizeChanged += WindowOnSizeChanged;
    }

    private void WindowOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // persist
        if (_settings == null)
        {
            return;
        }

        _settings.MenuWidth = e.NewSize.Width;
        _settings.MenuHeight = e.NewSize.Height;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _container.GetInstance<MainWindow>().SizeChanged -= WindowOnSizeChanged;

        if (_modules != null)
        {
            StopModules(_modules);
        }

        _hotkey?.Unregister();

// #pragma warning disable CA1416 // Validate platform compatibility
        _notifyIcon?.Dispose();
// #pragma warning restore CA1416 // Validate platform compatibility

        base.OnExit(e);
    }

    private static IConfiguration SetupConfiguration(IFileSystem fileSystem)
    {
        const string FILENAME = "appsettings.serilog.json";
        var fullFilename = Path.Combine(DefaultAppDataPathProvider.Instance.GetAppDataPath(), FILENAME);
        if (!fileSystem.File.Exists(fullFilename))
        {
            fullFilename = FILENAME;
        }

        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(fullFilename, optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
        return builder.Build();
    }

    private static ILoggerFactory CreateLoggerFactory(IConfiguration config)
    {
        ILoggerFactory loggerFactory = new LoggerFactory();

        LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(config);

        Logger logger = loggerConfiguration.CreateLogger();

        _ = loggerFactory.AddSerilog(logger);

        return loggerFactory;
    }

    private static void RegisterLogging(ILoggerFactory loggerFactory)
    {
        // https://stackoverflow.com/questions/41243485/simple-injector-register-iloggert-by-using-iloggerfactory-createloggert

        _container.RegisterInstance<ILoggerFactory>(loggerFactory);
        _container.RegisterSingleton(typeof(ILogger<>), typeof(Logger<>));

        _container.RegisterConditional(
            typeof(ILogger),
            c => c.Consumer == null
                ? typeof(Logger<object>)
                : typeof(Logger<>).MakeGenericType(c.Consumer.ImplementationType),
            Lifestyle.Singleton,
            _ => true);
    }

    private static void RegisterServices(Container container, IFileSystem fileSystem)
    {
        container.Register<MainWindow>(Lifestyle.Singleton);
        container.Register<StatusCharacterMap>(Lifestyle.Singleton);
        container.Register<StatusCompressor>(Lifestyle.Singleton);
        container.Register<IRepositoryInformationAggregator, DefaultRepositoryInformationAggregator>(Lifestyle.Singleton);
        container.Register<IRepositoryMonitor, DefaultRepositoryMonitor>(Lifestyle.Singleton);
        container.Register<IRepositoryDetectorFactory, DefaultRepositoryDetectorFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryObserverFactory, DefaultRepositoryObserverFactory>(Lifestyle.Singleton);
        container.Register<IGitRepositoryFinderFactory, GitRepositoryFinderFactory>(Lifestyle.Singleton);
        container.RegisterInstance<IAppDataPathProvider>(DefaultAppDataPathProvider.Instance);
        container.Register<IRepositoryActionProvider, DefaultRepositoryActionProvider>(Lifestyle.Singleton);
        container.Register<IRepositoryReader, DefaultRepositoryReader>(Lifestyle.Singleton);
        container.Register<IRepositoryWriter, DefaultRepositoryWriter>(Lifestyle.Singleton);
        container.Register<IRepositoryStore, DefaultRepositoryStore>(Lifestyle.Singleton);
        container.Register<IPathProvider, DefaultDriveEnumerator>(Lifestyle.Singleton);
        container.Register<IPathSkipper, WindowsPathSkipper>(Lifestyle.Singleton);
        container.Register<IThreadDispatcher, WpfThreadDispatcher>(Lifestyle.Singleton);
        container.Register<IGitCommander, ProcessExecutingGitCommander>(Lifestyle.Singleton);
        container.Register<IAppSettingsService, FileAppSettingsService>(Lifestyle.Singleton);
        container.Register<ICompareSettingsService, FilesCompareSettingsService> (Lifestyle.Singleton);
        container.Register<IFilterSettingsService, FilesFilterSettingsService> (Lifestyle.Singleton);
        container.Register<IAutoFetchHandler, DefaultAutoFetchHandler>(Lifestyle.Singleton);
        container.Register<IRepositoryIgnoreStore, DefaultRepositoryIgnoreStore>(Lifestyle.Singleton);
        container.Register<ITranslationService, ResourceDictionaryTranslationService>(Lifestyle.Singleton);
        container.RegisterInstance<IClock>(SystemClock.Instance);
        container.Register<IRepositoryTagsFactory, RepositoryTagsConfigurationFactory>(Lifestyle.Singleton);
        container.Register<RepositoryConfigurationReader>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerManager, RepositoryComparerManager>(Lifestyle.Singleton);

        container.Register<IRepositoryMatcher, RepositoryMatcher>(Lifestyle.Singleton);
        container.Register<IRepositoryFilteringManager, RepositoryFilteringManager>(Lifestyle.Singleton);
        container.Collection.Append<INamedQueryParser, DefaultQueryParser>(Lifestyle.Singleton);

        container.Collection.Append<IQueryMatcher, IsPinnedMatcher>(Lifestyle.Singleton);
        container.Collection.Append<IQueryMatcher, TagMatcher>(Lifestyle.Singleton);
        container.Collection.Append<IQueryMatcher, HasUnPushedChangesMatcher>(Lifestyle.Singleton);
        container.Collection.Append<IQueryMatcher>(() => new FreeTextMatcher(ignoreCase: true, ignoreCaseTag: true), Lifestyle.Singleton);


        container.Collection.Append<ISingleGitRepositoryFinderFactory, GravellGitRepositoryFinderFactory>(Lifestyle.Singleton);

        container.RegisterInstance<IFileSystem>(fileSystem);

        container.Register<IRepositoryExpressionEvaluator, RepositoryExpressionEvaluator>(Lifestyle.Singleton);
        Assembly[] repoExpressionEvaluators =
            {
                typeof(IVariableProvider).Assembly,
                typeof(RepositoryExpressionEvaluator).Assembly,
                typeof(IRepositoryExpressionEvaluator).Assembly,
            };
        
        container.Collection.Append<IVariableProvider, DateTimeNowVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, DateTimeTimeVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, DateTimeDateVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, EmptyVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, CustomEnvironmentVariableVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, RepoMVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, RepositoryVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, SlashVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, BackslashVariableProvider>(Lifestyle.Singleton);
        container.Collection.Append<IVariableProvider, VariableProviderAdapter>(Lifestyle.Singleton);

        container.Collection.Register(typeof(IMethod), repoExpressionEvaluators, Lifestyle.Singleton);
        container.RegisterInstance(new DateTimeVariableProviderOptions
            {
                DateTimeProvider = () => DateTime.Now,
            });
        container.RegisterInstance(new DateTimeNowVariableProviderOptions
            {
                DateTimeProvider = () => DateTime.Now,
            });
        container.RegisterInstance(new DateTimeDateVariableProviderOptions
            {
                DateTimeProvider = () => DateTime.Now,
            });

        container.Register<ActionDeserializerComposition>(Lifestyle.Singleton);
        container.Collection.Register<IActionDeserializer>(
            new[] { typeof(IActionDeserializer).Assembly, },
            Lifestyle.Singleton);

        container.Register<ActionMapperComposition>(Lifestyle.Singleton);
        container.Collection.Register<IActionToRepositoryActionMapper>(
            new[] { typeof(IActionToRepositoryActionMapper).Assembly, },
            Lifestyle.Singleton);
        
        container.Register<JsonDynamicRepositoryActionDeserializer>(Lifestyle.Singleton);
        container.Register<YamlDynamicRepositoryActionDeserializer>(Lifestyle.Singleton);
        container.Register<RepositorySpecificConfiguration>(Lifestyle.Singleton);


        container.RegisterSingleton<IRepositoryComparerFactory, RepositoryComparerCompositionFactory>();
        container.RegisterSingleton<IRepositoryScoreCalculatorFactory, RepositoryScoreCalculatorFactory>();

        container.Collection.Register(
            typeof(IConfigurationRegistration),
            new[] { typeof(IConfigurationRegistration).Assembly, typeof(IsPinnedScorerConfigurationV1Registration).Assembly, },
            Lifestyle.Singleton);

        container.Register<IRepositoryScoreCalculatorFactory<IsPinnedScorerConfigurationV1>, IsPinnedScorerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryScoreCalculatorFactory<TagScorerConfigurationV1>, TagScorerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<AlphabetComparerConfigurationV1>, AzRepositoryComparerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<CompositionComparerConfigurationV1>, CompositionRepositoryComparerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<ScoreComparerConfigurationV1>, ScoreRepositoryComparerFactory>(Lifestyle.Singleton);
        container.Register<IRepositoryComparerFactory<SumComparerConfigurationV1>, SumRepositoryComparerFactory>(Lifestyle.Singleton);

        container.RegisterSingleton<ActionExecutor>();
        container.Register(typeof(IActionExecutor<>), new [] { typeof(BrowseActionExecutor).Assembly, }, Lifestyle.Singleton);
        container.RegisterDecorator(
            typeof(IActionExecutor<>),
            typeof(LoggerActionExecutorDecorator<>),
            Lifestyle.Singleton);
        
        IEnumerable<FileInfo> pluginDlls = PluginFinder.FindPluginAssemblies(Path.Combine(AppDomain.CurrentDomain.BaseDirectory), fileSystem);
        IEnumerable<Assembly> assemblies = pluginDlls.Select(plugin => Assembly.Load(AssemblyName.GetAssemblyName(plugin.FullName)));
        container.RegisterPackages(assemblies);
    }

    private static void StartModules(List<IModule> modules)
    {
        var allTasks = Task.WhenAll(modules.Select(x => x.StartAsync()));
        allTasks.GetAwaiter().GetResult();
    }

    private static void StopModules(List<IModule> modules)
    {
        var task = Task.Run(() =>
            {
                return Task.WhenAll(modules.Select(async module =>
                    {
                        await module.StopAsync().ConfigureAwait(false);

                        if (module is IAsyncDisposable asyncDisposable)
                        {
                            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        }
                        else if (module is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }));
            });

        task.ConfigureAwait(false).GetAwaiter().GetResult();
    }
    private static void UseRepositoryMonitor(Container container)
    {
        // var repositoryInformationAggregator = container.GetInstance<IRepositoryInformationAggregator>();
        _repositoryMonitor = container.GetInstance<IRepositoryMonitor>();
        _repositoryMonitor.Observe();
    }

    private static async Task CheckForUpdatesAsync()
    {
        await Task.Yield();
        AvailableUpdate = null;
        _updateTimer?.Change((int)TimeSpan.FromHours(2).TotalMilliseconds, Timeout.Infinite);
    }

    private static void EnsureWindowHandle(Window window)
    {
        // We noticed that the hotkey registration at app start causes a high CPU utilization if the main window was not shown before.
        // To fix this, we need to make the window visible. However, to prevent flickering we move the window out of the screen bounds to show and hide it.
        window.Left = -9999;
        window.Show();
        window.Hide();
    }

    private static void OnHotKeyPressed()
    {
        (Application.Current.MainWindow as MainWindow)?.ShowAndActivate();
    }

    public static string? AvailableUpdate { get; private set; }
}