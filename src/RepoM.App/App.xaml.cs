[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.App.i18n;
using RepoM.Core.Plugin;
using SimpleInjector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using RepoM.App.Services;
using Container = SimpleInjector.Container;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private Timer? _updateTimer;
    private HotKey? _hotkey;
    private static IRepositoryMonitor? _repositoryMonitor;
    private TaskbarIcon? _notifyIcon;
    private List<IModule>? _modules;
    private IAppSettingsService? _settings;

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
        Bootstrapper.RegisterLogging(loggerFactory);
        Bootstrapper.RegisterServices(fileSystem);
        Bootstrapper.RegisterPlugins(fileSystem);

#if DEBUG
        Bootstrapper.Container.Verify(VerificationOption.VerifyAndDiagnose);
#endif

        UseRepositoryMonitor(Bootstrapper.Container);

        _updateTimer = new Timer(async _ => await CheckForUpdatesAsync(), null, 5000, Timeout.Infinite);

        // We noticed that the hotkey registration causes a high CPU utilization if the window was not shown before.
        // To fix this, we need to make the window visible in EnsureWindowHandle() but we set the opacity to 0.0 to prevent flickering
        MainWindow window = Bootstrapper.Container.GetInstance<MainWindow>();
        EnsureWindowHandle(window);

        _hotkey = new HotKey(47110815);
        _hotkey.Register(window, HotKey.VK_R, HotKey.MOD_ALT | HotKey.MOD_CTRL, OnHotKeyPressed);

        _modules = Bootstrapper.Container.GetAllInstances<IModule>().ToList();
        StartModules(_modules);

        _settings = Bootstrapper.Container.GetInstance<IAppSettingsService>();

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
        Bootstrapper.Container.GetInstance<MainWindow>().SizeChanged -= WindowOnSizeChanged;

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
        // var repositoryInformationAggregator = Container.GetInstance<IRepositoryInformationAggregator>();
        _repositoryMonitor = container.GetInstance<IRepositoryMonitor>();
        _repositoryMonitor.Observe();
    }

    private async Task CheckForUpdatesAsync()
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