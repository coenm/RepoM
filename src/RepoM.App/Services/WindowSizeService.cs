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

internal sealed class WindowSizeService : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly IAppSettingsService _appSettings;
    private IDisposable? _registrationWindowSizeChanged;
    private IDisposable? _registrationDisplaySettingsChanged;
    private readonly SynchronizationContext _uiDispatcher;
    private static readonly TimeSpan _throttle = TimeSpan.FromSeconds(5);

    public WindowSizeService(MainWindow mainWindow, IAppSettingsService appSettings)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _uiDispatcher = SynchronizationContext.Current!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetResolution()
    {
        Screen? screen = Screen.PrimaryScreen;
        return screen == null ? string.Empty : $"{screen.Bounds.Width}x{screen.Bounds.Height}";
    }

    public void Register()
    {
        if (_appSettings.TryGetMenuSize(GetResolution(), out MenuSize? size))
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
                GetResolution(),
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
            .Select(eventPattern =>
                {
                    _ = _appSettings.TryGetMenuSize(GetResolution(), out MenuSize? menuSize);
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
            .Select(eventPattern => new CustomSizeChangedEventArgs(eventPattern.EventArgs, GetResolution()))
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
}

file class CustomSizeChangedEventArgs : RoutedEventArgs
{
    public CustomSizeChangedEventArgs(SizeChangedEventArgs sizeChangedEventArgs, string resolution)
    {
        Resolution = resolution;
        NewSize = sizeChangedEventArgs.NewSize;
    }

    public Size NewSize { get; init; }

    public string Resolution { get; init; }
}