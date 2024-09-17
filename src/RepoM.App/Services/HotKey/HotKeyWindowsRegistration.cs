namespace RepoM.App.Services.HotKey;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

internal partial class HotKeyWindowsRegistration
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

    private IntPtr _handle;
    private Action? _hotKeyActionToCall;
    private readonly int _id;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HotKeyWindowsRegistration(int id)
    {
        _id = id;
    }

    public void Register(Window window, uint key, uint modifiers, Action hotKeyActionToCall)
    {
        var helper = new WindowInteropHelper(window);
        _handle = helper.EnsureHandle();
        _hotKeyActionToCall = hotKeyActionToCall;

        var source = HwndSource.FromHwnd(_handle);
        source?.AddHook(HwndHook);

        var ok = RegisterHotKey(_handle, _id, modifiers, key);
        if (!ok)
        {
            // handle error
        }
    }

    public void Unregister()
    {
        UnregisterHotKey(_handle, _id);
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOT_KEY = 0x0312;
        switch (msg)
        {
            case WM_HOT_KEY:
                if (wParam.ToInt32() == _id)
                {
                    _hotKeyActionToCall?.Invoke();
                    handled = true;
                }

                break;
        }

        return IntPtr.Zero;
    }
}