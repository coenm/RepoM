namespace RepoM.App.Services;

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using RepoM.Api.Common;

internal class WindowSizeService: IDisposable
{
    private const string UNKNOWN_RESOLUTION    = "unknown";
    private volatile string _currentResolution = UNKNOWN_RESOLUTION;
    private readonly Window _mainWindow;
    private readonly IAppSettingsService _appSettings;
    private readonly ILogger _logger;
    private IDisposable? _registrationWindowSizeChanged;
    private IDisposable? _registrationDisplaySettingsChanged;
    private readonly SynchronizationContext _uiDispatcher;
    protected static readonly TimeSpan ThrottleWindowSizeChanged      = TimeSpan.FromSeconds(5);
    protected static readonly TimeSpan ThrottleDisplaySettingsChanged = TimeSpan.FromSeconds(1);

    public WindowSizeService(Window mainWindow, IAppSettingsService appSettings, IThreadDispatcher threadDispatcher, ILogger logger)
    {
        _mainWindow  = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _logger      = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(threadDispatcher);
        _uiDispatcher = threadDispatcher.SynchronizationContext;
    }

    public void Register()
    {
        _currentResolution = GetResolution();

        if (_appSettings.TryGetMenuSize(_currentResolution, out MenuSize? size))
        {
            _mainWindow.SetCurrentValue(FrameworkElement.WidthProperty, size.Value.MenuWidth);
            _mainWindow.SetCurrentValue(FrameworkElement.HeightProperty, size.Value.MenuHeight);
        }
        else
        {
            _appSettings.UpdateMenuSize(
                _currentResolution,
                new MenuSize
                    {
                        MenuHeight = _mainWindow.Height,
                        MenuWidth  = _mainWindow.Width,
                    });
        }

        _registrationDisplaySettingsChanged = Observable
            .FromEventPattern<EventHandler, EventArgs>(
              handler => SystemEvents.DisplaySettingsChanged += handler,
              handler => SystemEvents.DisplaySettingsChanged -= handler)
            .ObserveOn(Scheduler.Default)
            .Throttle(ThrottleDisplaySettingsChanged)
            .Select(eventPattern =>
                {
                    try
                    {
                          // update resolution in select is not very nice.
                        _currentResolution = GetResolution();

                        _ = _appSettings.TryGetMenuSize(_currentResolution, out MenuSize? menuSize);
                        return menuSize;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Could not get resolution or menu for current screen.");
                        return null;
                    }
                })
            .Where(menuSize => menuSize.HasValue)
            .Select(menuSize => menuSize!.Value)
            .ObserveOn(_uiDispatcher)  // Accessing the mainWindow should be done from UI thread.
            .Where(menuSize =>
                {
                    try
                    {
                        return Math.Abs(_mainWindow.Width - menuSize.MenuWidth) > 0.001
                               ||
                               Math.Abs(_mainWindow.Height - menuSize.MenuHeight) > 0.001;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Could not determine if window size has changed.");
                        return true;
                    }

                })
            .Subscribe(menuSize =>
                {
                    _mainWindow.SetCurrentValue(FrameworkElement.WidthProperty,  menuSize!.MenuWidth);
                    _mainWindow.SetCurrentValue(FrameworkElement.HeightProperty, menuSize!.MenuHeight);
                });

        _registrationWindowSizeChanged = Observable
            .FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(
                handler => _mainWindow.SizeChanged += handler,
                handler => _mainWindow.SizeChanged -= handler)
            .ObserveOn(Scheduler.Default)
            .Throttle(ThrottleWindowSizeChanged)
            .Subscribe(sizeChangedEvent =>
                {

                    _appSettings.UpdateMenuSize(
                        _currentResolution,  // Yes, This possibility can go wrong
                        new MenuSize
                        {
                            MenuHeight = sizeChangedEvent.EventArgs.NewSize.Height,
                            MenuWidth  = sizeChangedEvent.EventArgs.NewSize.Width,
                        });
                });

        _currentResolution = GetResolution();
    }

    public void Unregister()
    {
        Dispose();
    }

    public void Dispose()
    {
        _registrationWindowSizeChanged?.Dispose();
        _registrationWindowSizeChanged = null;
        _registrationDisplaySettingsChanged?.Dispose();
        _registrationDisplaySettingsChanged = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual string GetResolution()
    {
        Screen? screen  = Screen.PrimaryScreen;
        return  screen == null ? UNKNOWN_RESOLUTION : $"{screen.Bounds.Width}x{screen.Bounds.Height}";
    }
}
