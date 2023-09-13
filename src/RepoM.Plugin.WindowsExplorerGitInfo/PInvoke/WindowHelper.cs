namespace RepoM.Plugin.WindowsExplorerGitInfo.PInvoke;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Reviewed")]
[SuppressMessage("ReSharper", "IdentifierTypo")]
internal static class WindowHelper
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Auto)]
    private static extern bool SetWindowTextApi(IntPtr hWnd, string strNewWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, [Out] StringBuilder lParam);

    public static IntPtr FindActiveWindow()
    {
        return GetForegroundWindow();
    }

    private const uint WM_GETTEXT = 0x000D;
    private const uint WM_GETTEXTLENGTH = 0x000E;

    public static string GetWindowText(IntPtr hwnd)
    {
        // Allocate correct string length first
        var length = (int)SendMessage(hwnd, WM_GETTEXTLENGTH, IntPtr.Zero, null!);
        var sb = new StringBuilder(length + 1);
        SendMessage(hwnd, WM_GETTEXT, (IntPtr)sb.Capacity, sb);
        return sb.ToString();
    }

    public static void SetWindowText(IntPtr handle, string text)
    {
        if (handle != IntPtr.Zero && text != null)
        {
            SetWindowTextApi(handle, text);
        }
    }

    public static void AppendWindowText(IntPtr handle, string uniqueSplitter, string text)
    {
        var current = GetWindowText(handle);

        var at = current.IndexOf(uniqueSplitter, StringComparison.OrdinalIgnoreCase);
        if (at > -1)
        {
            current = current[..at];
        }

        SetWindowTextApi(handle, current + uniqueSplitter + text);
    }

    public static void RemoveAppendedWindowText(IntPtr handle, string uniqueSplitter)
    {
        var current = GetWindowText(handle);

        var at = current.IndexOf(uniqueSplitter, StringComparison.OrdinalIgnoreCase);
        if (at <= -1)
        {
            return;
        }

        current = current[..at];
        SetWindowTextApi(handle, current);
    }
}