namespace RepoM.App.Services;

using System;
using System.Windows;
using RepoM.Api.Common;

internal class WindowSizeService
{
    private readonly MainWindow _mainWindow;
    private readonly IAppSettingsService _appSettings;

    public WindowSizeService(MainWindow mainWindow, IAppSettingsService appSettings)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    public void Register()
    {
        if (_appSettings.MenuWidth > 0)
        {
            _mainWindow.Width = _appSettings.MenuWidth;
        }

        if (_appSettings.MenuHeight > 0)
        {
            _mainWindow.Height = _appSettings.MenuHeight;
        }

        _mainWindow.SizeChanged += WindowOnSizeChanged;
    }

    public void Unregister()
    {
        _mainWindow.SizeChanged -= WindowOnSizeChanged;
    }

    private void WindowOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // persist
        _appSettings.MenuWidth = e.NewSize.Width;
        _appSettings.MenuHeight = e.NewSize.Height;
    }
}