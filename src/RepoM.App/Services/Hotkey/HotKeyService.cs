namespace RepoM.App.Services.Hotkey;

using System;
using System.Windows;

internal class HotKeyService
{
    private readonly MainWindow _mainWindow;
    private HotKey? _hotKey;

    public HotKeyService(MainWindow mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    public void Register()
    {
        // We noticed that the hotkey registration causes a high CPU utilization if the window was not shown before.
        // To fix this, we need to make the window visible in EnsureWindowHandle() but we set the opacity to 0.0 to prevent flickering
        EnsureWindowHandle(_mainWindow);

        _hotKey = new HotKey(47110815);
        _hotKey.Register(_mainWindow, HotKey.VK_R, HotKey.MOD_ALT | HotKey.MOD_CTRL, OnHotKeyPressed);
    }

    public void Unregister()
    {
        _hotKey?.Unregister();
    }

    private static void EnsureWindowHandle(Window window)
    {
        // We noticed that the hotkey registration at app start causes a high CPU utilization if the main window was not shown before.
        // To fix this, we need to make the window visible. However, to prevent flickering we move the window out of the screen bounds to show and hide it.
        window.Left = -9999;
        window.Show();
        window.Hide();
    }

    private void OnHotKeyPressed()
    {
        _mainWindow.ShowAndActivate();
    }
}