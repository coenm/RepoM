[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace RepoM.App;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using RepoM.UI.i18n;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RepoM.Api.Plugins;
using RepoM.App.Plugins;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using RepoM.App.Services;
using RepoM.UI.Services;
using RepoM.UI.Services.HotKey;
using RepoM.Api;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Common;
using RepoM.App.Startup;
using RepoM.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly Bootstrapper _bootstrapper = new();
    private StartupDependencies? _startupDependencies;
    private ILoggerFactory? _loggerFactory;
    private IApplicationWindow? _applicationWindow;
    private StartupArguments _arguments = new(null, []);
    private static Mutex? _mutex;
    private TaskbarIcon? _notifyIcon;
    private ModuleService? _moduleService;
    private HotKeyService? _hotKeyService;
    private WindowSizeService? _windowSizeService;
#if DEBUG
    private const bool CHECK_SINGLE_INSTANCE = false;
#else
    private const bool CHECK_SINGLE_INSTANCE = true;
#endif

    [STAThread]
    public static void Main(string[] args)
    {
        StartupArguments arguments;
        try
        {
            arguments = StartupArguments.Parse(args);
        }
        catch (ArgumentException exception)
        {
            MessageBox.Show(exception.Message, "RepoM startup", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.ExitCode = 1;
            return;
        }

        if (arguments.PluginPath == null && IsAlreadyRunning())
        {
            return;
        }

        Thread.CurrentThread.Name ??= "UI";
        var app = new App { _arguments = arguments };
        app.InitializeComponent();
        Environment.ExitCode = app.Run();
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

        try
        {
            await StartApplicationAsync();
        }
        catch (Exception exception)
        {
            _loggerFactory?.CreateLogger(nameof(App)).LogCritical(exception, "Application startup failed.");
            System.Diagnostics.Trace.TraceError("Application startup failed: {0}", exception);
            MessageBox.Show(exception.Message, "RepoM startup failed", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private async Task StartApplicationAsync()
    {
        Resources.MergedDictionaries.Add(ResourceDictionaryTranslationService.ResourceDictionary);
        _startupDependencies = await StartupDependencies.CreateAsync(_arguments);
        IFileSystem fileSystem = _startupDependencies.FileSystem;
        IPluginFinder pluginFinder = _startupDependencies.PluginFinder;
        IAppDataPathProvider appDataProvider = await _startupDependencies.PathProviderFactory.CreateAsync();

        IConfiguration config = LoggingFactory.CreateLoggerConfiguration(appDataProvider.AppDataPath);
        ILoggerFactory loggerFactory = LoggingFactory.CreateLoggerFactory(config);
        _loggerFactory = loggerFactory;

        ILogger logger = loggerFactory.CreateLogger(nameof(App));
        RegisterGlobalExceptionHandlers(logger);
        logger.LogInformation("Started");
        _bootstrapper.RegisterLogging(loggerFactory);
        _bootstrapper.RegisterServices(fileSystem, appDataProvider);
        _startupDependencies.RegisterInstances(_bootstrapper.Container);

        // Move heavy I/O work (plugin discovery, assembly loading, file ensures) off the UI thread
        // so the message pump stays responsive during startup.
        await Task.Run(async () =>
        {
            await _bootstrapper.RegisterPlugins(pluginFinder, fileSystem, loggerFactory, appDataProvider).ConfigureAwait(false);

            var ensureStartup = new EnsureStartup(fileSystem, appDataProvider);
            await ensureStartup.EnsureFilesAsync().ConfigureAwait(false);
        }).ConfigureAwait(true);

#if DEBUG
        _bootstrapper.Container.Verify(SimpleInjector.VerificationOption.VerifyAndDiagnose);
#else
        _bootstrapper.Container.Verify(SimpleInjector.VerificationOption.VerifyOnly);
#endif

        _moduleService = _bootstrapper.Container.GetInstance<ModuleService>();
        var presentation = _bootstrapper.Container.GetInstance<IApplicationPresentation>();
        if (presentation.UseTray)
        {
            _notifyIcon = FindResource("NotifyIcon") as TaskbarIcon;
            _hotKeyService = _bootstrapper.Container.GetInstance<HotKeyService>();
            _windowSizeService = _bootstrapper.Container.GetInstance<WindowSizeService>();
            _hotKeyService.Register();
            _windowSizeService.Register();
        }
        else
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        _applicationWindow = _bootstrapper.Container.GetInstance<IApplicationWindow>();
        MainWindow = _applicationWindow.Window;

        // Make the UI interactive immediately — don't wait for modules to finish loading.
        _applicationWindow.SetReady();
        if (!presentation.UseTray)
        {
            _applicationWindow.ShowAndActivate();
        }

        // Start modules in the background; repository data will stream in via DynamicData subscriptions.
        _ = _moduleService.StartAsync().ContinueWith(
            t => logger.LogError(t.Exception, "Could not start all modules."),
            TaskContinuationOptions.OnlyOnFaulted);

        // Warm up the action-menu pipeline in the background so the first context-menu open is fast.
        IUserMenuActionMenuFactory userMenuActionMenuFactory = _bootstrapper.Container.GetInstance<IUserMenuActionMenuFactory>();
        _ = Task.Run(userMenuActionMenuFactory.WarmupAsync).ContinueWith(
            t => logger.LogError(t.Exception, "Could not warm up the action menu."),
            TaskContinuationOptions.OnlyOnFaulted);
    }

    private void RegisterGlobalExceptionHandlers(ILogger logger)
    {
        // Keeps the tray app alive when a UI handler (e.g. an async void event handler) throws.
        DispatcherUnhandledException += (_, args) =>
        {
            logger.LogError(args.Exception, "Unhandled exception on the UI thread.");
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            logger.LogCritical(args.ExceptionObject as Exception, "Unhandled exception (terminating: {IsTerminating}).", args.IsTerminating);

        // Only raised when a faulted, never-awaited task is garbage collected.
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            logger.LogError(args.Exception, "Unobserved task exception.");
            args.SetObserved();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _applicationWindow?.Shutdown();

        _windowSizeService?.Unregister();

        _moduleService?.StopAsync().GetAwaiter().GetResult();

        _hotKeyService?.Unregister();

        _notifyIcon?.Dispose();

        _bootstrapper.Container.DisposeContainerAsync().GetAwaiter().GetResult();
        _startupDependencies?.Dispose();
        _loggerFactory?.Dispose();

        ReleaseAndDisposeMutex();

        base.OnExit(e);
    }

    [SuppressMessage("Major Code Smell", "S2589:Boolean expressions should not be gratuitous", Justification = "Compiler condition")]
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode", Justification = "Compiler condition")]
    private static bool IsAlreadyRunning()
    {
        if (!CHECK_SINGLE_INSTANCE)
        {
            return false;
        }

#pragma warning disable CS0162 // Unreachable code detected - intentional in DEBUG mode
        try
        {
            _mutex = new Mutex(true, "Local\\github.com/coenm/RepoM", out var createdNew);

            if (createdNew)
            {
                return false;
            }
        }
        catch (Exception)
        {
            return true;
        }

        _mutex.Dispose();
        _mutex = null;
        return true;
#pragma warning restore CS0162
    }

    private static void ReleaseAndDisposeMutex()
    {
        try
        {
            _mutex?.ReleaseMutex();
        }
        catch (Exception)
        {
            // ignore
        }

        try
        {
            _mutex?.Dispose();
        }
        catch (Exception)
        {
            // ignore
        }
    }

}
