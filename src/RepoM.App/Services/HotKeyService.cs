namespace RepoM.App.Services.HotKey;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;
using Key = System.Windows.Input.Key;

internal partial class HotKeyService
{

    [LibraryImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [LibraryImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(
        IntPtr hWnd,
        int id);

    private const int HOT_KEY_ID = 47110815;
    private readonly IntPtr _hotKeyHook;
    private readonly Action? _hotKeyActionToCall;
    private readonly MainWindow _mainWindow;

    private readonly ILogger _logger;

    public HotKeyService(MainWindow mainWindow, ILogger logger)
    {
        _mainWindow = mainWindow;
        _logger = logger;
        _hotKeyActionToCall = OnHotKeyPressed;  // This is the function that will ultimately be called when the hotkey is pressed

        var helper  = new WindowInteropHelper(_mainWindow); // This is the window that will receive the hotkey message
        _hotKeyHook = helper.EnsureHandle();  // This is the handle of the window that will receive the hotkey message

        var source = HwndSource.FromHwnd(_hotKeyHook);  // This is the source of the window that will receive the hotkey message
        source?.AddHook(HwndHook);  // This is the proxy function that will be called when the hotkey message is received
    }
    public void Register()
    {
        EnsureWindowHandle();

        var successHotKeyRegistration = RegisterHotKey(_hotKeyHook, HOT_KEY_ID, (uint)(ModifierKeys.Shift | ModifierKeys.Control), (uint)KeyInterop.VirtualKeyFromKey(Key.R));
        if (successHotKeyRegistration)
        {
            _logger.LogInformation("Hotkey registered successfully");
        }
        else
        {
            _logger.LogError("Hotkey registration failed");
        }
    }

    public void Unregister()
    {
        UnregisterHotKey(_hotKeyHook, HOT_KEY_ID);
    }

    private void EnsureWindowHandle()
    {
        // We noticed that the hotkey registration at app start causes a high CPU utilization if the main window was not shown before.
        // To fix this, we need to make the window visible. However, to prevent flickering we move the window out of the screen bounds to show and hide it.
        _mainWindow.SetCurrentValue(Window.LeftProperty, -9999.0);
        _mainWindow.Show();
        _mainWindow.Hide();
    }

    private void OnHotKeyPressed()
    {
        _mainWindow.ShowAndActivate();
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOT_KEY = 0x0312;
        switch (msg)
        {
            case WM_HOT_KEY:
                if (wParam.ToInt32() == HOT_KEY_ID)
                {
                    _hotKeyActionToCall?.Invoke();
                    handled = true;
                }

                break;
        }

        return IntPtr.Zero;
    }
}
