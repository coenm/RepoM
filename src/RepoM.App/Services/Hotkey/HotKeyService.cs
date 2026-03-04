namespace RepoM.App.Services.HotKey;

using System;

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
        // HotKeyWindowsRegistration.Register() calls WindowInteropHelper.EnsureHandle()
        // which creates the native HWND without a full WPF render pass.
        _hotKey = new HotKeyWindowsRegistration(47110815);
        _hotKey.Register(_mainWindow, HotKeyWindowsRegistration.VK_R, HotKeyWindowsRegistration.MOD_ALT | HotKeyWindowsRegistration.MOD_CTRL, OnHotKeyPressed);
    }

    public void Unregister()
    {
        _hotKey?.Unregister();
    }

    private void OnHotKeyPressed()
    {
        _mainWindow.ShowAndActivate();
    }
}