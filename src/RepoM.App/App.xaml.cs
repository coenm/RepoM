[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace RepoM.App;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.App.i18n;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepoM.Api.Plugins;
using RepoM.App.Plugins;
using Serilog;
using Serilog.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using RepoM.App.Services;
using Container = SimpleInjector.Container;
using RepoM.App.Services.HotKey;
using Serilog.Enrichers;
using RepoM.Api;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IRepositoryMonitor? _repositoryMonitor;
    private TaskbarIcon? _notifyIcon;
    private ModuleService? _moduleService;
    private HotKeyService? _hotKeyService;
    private WindowSizeService? _windowSizeService;

    [STAThread]
    public static void Main()
    {
        Thread.CurrentThread.Name ??= "UI";
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    protected override async void OnStartup(StartupEventArgs e)
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
        IHmacService hmacService = new HmacSha256Service();
        IPluginFinder pluginFinder = new PluginFinder(fileSystem, hmacService);

        IConfiguration config = SetupConfiguration();
        ILoggerFactory loggerFactory = CreateLoggerFactory(config);
        ILogger logger = loggerFactory.CreateLogger(nameof(App));
        logger.LogInformation("Started");
        Bootstrapper.RegisterLogging(loggerFactory);
        Bootstrapper.RegisterServices(fileSystem);
        await Bootstrapper.RegisterPlugins(pluginFinder, fileSystem, loggerFactory).ConfigureAwait(true);

#if DEBUG
        Bootstrapper.Container.Verify(SimpleInjector.VerificationOption.VerifyAndDiagnose);
#else
        Bootstrapper.Container.Options.EnableAutoVerification = false;
#endif

        EnsureStartup ensureStartup = Bootstrapper.Container.GetInstance<EnsureStartup>();
        await ensureStartup.EnsureFilesAsync().ConfigureAwait(true);
        
        UseRepositoryMonitor(Bootstrapper.Container);

        _moduleService = Bootstrapper.Container.GetInstance<ModuleService>();
        _hotKeyService = Bootstrapper.Container.GetInstance<HotKeyService>();
        _windowSizeService = Bootstrapper.Container.GetInstance<WindowSizeService>();

        _hotKeyService.Register();
        _windowSizeService.Register();

        try
        {
            await _moduleService.StartAsync().ConfigureAwait(false); // don't care about ui thread
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not start all modules.");
        }
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

    private static IConfiguration SetupConfiguration()
    {
        const string FILENAME = "appsettings.serilog.json";
        var fullFilename = Path.Combine(DefaultAppDataPathProvider.Instance.AppDataPath, FILENAME);

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
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithProperty("ThreadName", "BG")
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

    public static string? AvailableUpdate { get; private set; } = null;
}