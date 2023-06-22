[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace RepoM.App;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.App.i18n;
using SimpleInjector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepoM.App.Plugins;
using Serilog;
using Serilog.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using RepoM.App.Services;
using Container = SimpleInjector.Container;
using RepoM.App.Services.Hotkey;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IRepositoryMonitor? _repositoryMonitor;
    private Timer? _updateTimer;
    private TaskbarIcon? _notifyIcon;
    private ModuleService? _moduleService;
    private HotKeyService? _hotKeyService;
    private WindowSizeService? _windowSizeService;

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

        // Create instance without DI, because we need it before the last registration of services.
        IPluginFinder pluginFinder = new PluginFinder(fileSystem);

        IConfiguration config = SetupConfiguration(fileSystem);
        ILoggerFactory loggerFactory = CreateLoggerFactory(config);
        ILogger logger = loggerFactory.CreateLogger(nameof(App));
        logger.LogInformation("Started");
        Bootstrapper.RegisterLogging(loggerFactory);
        Bootstrapper.RegisterServices(fileSystem);
        Bootstrapper.RegisterPlugins(pluginFinder);

#if DEBUG
        Bootstrapper.Container.Verify(VerificationOption.VerifyAndDiagnose);
#endif

        UseRepositoryMonitor(Bootstrapper.Container);

        _updateTimer = new Timer(async _ => await CheckForUpdatesAsync(), null, 5000, Timeout.Infinite);

        MainWindow window = Bootstrapper.Container.GetInstance<MainWindow>();
        _moduleService = Bootstrapper.Container.GetInstance<ModuleService>();
        _hotKeyService = Bootstrapper.Container.GetInstance<HotKeyService>();
        _windowSizeService = Bootstrapper.Container.GetInstance<WindowSizeService>();

        _hotKeyService.Register();
        _windowSizeService.Register();
        _moduleService.StartAsync().GetAwaiter().GetResult();
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        _windowSizeService?.Unregister();
        
        _moduleService?.StopAsync().GetAwaiter().GetResult();

        _hotKeyService?.Unregister();

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

    private static void UseRepositoryMonitor(Container container)
    {
        _repositoryMonitor = container.GetInstance<IRepositoryMonitor>();
        _repositoryMonitor.Observe();
    }

    private async Task CheckForUpdatesAsync()
    {
        await Task.Yield();
        AvailableUpdate = null;
        _updateTimer?.Change((int)TimeSpan.FromHours(2).TotalMilliseconds, Timeout.Infinite);
    }

    public static string? AvailableUpdate { get; private set; }
}