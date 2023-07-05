namespace RepoM.App.Services;

using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

public static class AcrylicHelper
{
    public static void EnableBlur(Visual visual)
    {
        if (PresentationSource.FromVisual(visual) is HwndSource hwnd)
        {
            WindowsCompositionHelper.EnableBlur(hwnd.Handle);
        }
    }
}