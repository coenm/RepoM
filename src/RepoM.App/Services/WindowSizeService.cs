namespace RepoM.App.Services;

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using RepoM.Api.Common;

internal class WindowSizeService : IDisposable
{
    private const string UNKNOWN_RESOLUTION = "unknown";
    private volatile string _currentResolution = UNKNOWN_RESOLUTION;
    private readonly MainWindow _mainWindow;
    private readonly IAppSettingsService _appSettings;
    private IDisposable? _registrationWindowSizeChanged;
    private IDisposable? _registrationDisplaySettingsChanged;
    private readonly SynchronizationContext _uiDispatcher;
    private static readonly TimeSpan _throttleWindowSizeChanged = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _throttleDisplaySettingsChanged = TimeSpan.FromSeconds(1);

    public WindowSizeService(MainWindow mainWindow, IAppSettingsService appSettings, IThreadDispatcher threadDispatcher)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        ArgumentNullException.ThrowIfNull(threadDispatcher);
        _uiDispatcher = threadDispatcher.SynchronizationContext;
    }

    public void Register()
    {
        _currentResolution = GetResolution();
        
        if (_appSettings.TryGetMenuSize(_currentResolution, out MenuSize? size))
        {
            _mainWindow.Width = size.Value.MenuWidth;
            _mainWindow.Height = size.Value.MenuHeight;
        }
        else
        {
            if (_appSettings.MenuWidth is > 0)
            {
                _mainWindow.Width = _appSettings.MenuWidth.Value;
            }
            
            if (_appSettings.MenuHeight is > 0)
            {
                _mainWindow.Height = _appSettings.MenuHeight.Value;
            }

            _appSettings.UpdateMenuSize(
                _currentResolution,
                new MenuSize
                    {
                        MenuHeight = _mainWindow.Height,
                        MenuWidth = _mainWindow.Width,
                    });
        }
        
        _registrationDisplaySettingsChanged = Observable
            .FromEventPattern<EventHandler, EventArgs>(
              handler => SystemEvents.DisplaySettingsChanged += handler,
              handler => SystemEvents.DisplaySettingsChanged -= handler)
            .ObserveOn(Scheduler.Default)
            .Throttle(_throttleDisplaySettingsChanged)
            .Select(eventPattern =>
                {
                    // update resolution in select is not very nice.
                    _currentResolution = GetResolution(); 

                    _ = _appSettings.TryGetMenuSize(_currentResolution, out MenuSize? menuSize);
                    return menuSize;
                })
            .Where(menuSize => menuSize.HasValue
                               &&
                               (Math.Abs(_mainWindow.Width - menuSize.Value.MenuWidth) > 0.001
                                ||
                                Math.Abs(_mainWindow.Height - menuSize.Value.MenuHeight) > 0.001))
            .ObserveOn(_uiDispatcher)
            .Subscribe(menuSize =>
                {
                    _mainWindow.Width = menuSize!.Value.MenuWidth;
                    _mainWindow.Height = menuSize!.Value.MenuHeight;
                });
        
        _registrationWindowSizeChanged = Observable
            .FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(
                handler => _mainWindow.SizeChanged += handler,
                handler => _mainWindow.SizeChanged -= handler)
            .ObserveOn(Scheduler.Default)
            .Throttle(_throttleWindowSizeChanged)
            .Subscribe(sizeChangedEvent =>
                {
                    _appSettings.UpdateMenuSize(
                        _currentResolution, // Yes, This possibliy can go wrong
                        new MenuSize
                        {
                            MenuHeight = sizeChangedEvent.EventArgs.NewSize.Height,
                            MenuWidth = sizeChangedEvent.EventArgs.NewSize.Width,
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
        Screen? screen = Screen.PrimaryScreen;
        return screen == null ? UNKNOWN_RESOLUTION : $"{screen.Bounds.Width}x{screen.Bounds.Height}";
    }
}