namespace RepoM.App.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RepoM.Api.Git;

public partial class RepositoryComponent : UserControl
{
    public RepositoryComponent()
    {
        InitializeComponent();
    }

    private void MonitoringToggle_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is RepositoryViewModel vm)
        {
            vm.ToggleMonitoring();
            e.Handled = true;
        }
    }

    private void FavoriteToggle_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is RepositoryViewModel vm)
        {
            vm.ToggleFavorite();
            e.Handled = true;
        }
    }
}