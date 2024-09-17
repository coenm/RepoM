namespace RepoM.App.Services.HotKey;

using System;
using System.Windows;
using System.Windows.Input;

internal class HotKeyService
{
    private readonly MainWindow _mainWindow;
    private HotKeyWindowsRegistration? _hotKey;

    public HotKeyService(MainWindow mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
    }

    public void Register()
    {
        // We noticed that the hotkey registration causes a high CPU utilization if the window was not shown before.
        // To fix this, we need to make the window visible in EnsureWindowHandle() but we set the opacity to 0.0 to prevent flickering
        EnsureWindowHandle(_mainWindow);

        _hotKey = new HotKeyWindowsRegistration(47110815);
        _hotKey.Register(_mainWindow, (uint)VirtualKeyCode.VK_R, (uint)(ModifierKeys.Shift | ModifierKeys.Control), OnHotKeyPressed);
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