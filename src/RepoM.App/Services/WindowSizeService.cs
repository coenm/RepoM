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
using Size = System.Windows.Size;

internal class WindowSizeService : IDisposable
{
    private const string UNKNOWN_RESOLUTION = "unknown";
    private volatile string _currentResolution = UNKNOWN_RESOLUTION;
    private readonly MainWindow _mainWindow;
    private readonly IAppSettingsService _appSettings;
    private IDisposable? _registrationWindowSizeChanged;
    private IDisposable? _registrationDisplaySettingsChanged;
    private readonly SynchronizationContext _uiDispatcher;
    private static readonly TimeSpan _throttle = TimeSpan.FromSeconds(5);
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
            if (_appSettings.MenuWidth > 0)
            {
                _mainWindow.Width = _appSettings.MenuWidth;
            }
            
            if (_appSettings.MenuHeight > 0)
            {
                _mainWindow.Height = _appSettings.MenuHeight;
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
            .Where(menuSize => menuSize != null)
            .Select(menuSize => menuSize!.Value)
            .ObserveOn(_uiDispatcher)
            .Subscribe(menuSize =>
                {
                    _mainWindow.Width = menuSize.MenuWidth;
                    _mainWindow.Height = menuSize.MenuHeight;
                });
        
        _registrationWindowSizeChanged = Observable
            .FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(
                handler => _mainWindow.SizeChanged += handler,
                handler => _mainWindow.SizeChanged -= handler)
            .ObserveOn(Scheduler.Default)
            .Select(eventPattern => new CustomSizeChangedEventArgs(eventPattern.EventArgs, _currentResolution))
            .Throttle(_throttle)
            .Subscribe(sizeChangedEvent =>
                {
                    _appSettings.UpdateMenuSize(
                        sizeChangedEvent.Resolution,
                        new MenuSize
                        {
                            MenuHeight = sizeChangedEvent.NewSize.Height,
                            MenuWidth = sizeChangedEvent.NewSize.Width,
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

file class CustomSizeChangedEventArgs : RoutedEventArgs
{
    public CustomSizeChangedEventArgs(SizeChangedEventArgs sizeChangedEventArgs, string resolution)
    {
        Resolution = resolution;
        NewSize = sizeChangedEventArgs.NewSize;
    }

    public Size NewSize { get; }

    public string Resolution { get; }
}