namespace RepoM.App.Controls;

using System.Windows;
using System.Windows.Controls;
using RepoM.App.Services;

public class AcrylicContextMenu : ContextMenu
{
    protected override void OnOpened(RoutedEventArgs e)
    {
        base.OnOpened(e);

        AcrylicHelper.EnableBlur(this);
    }
}