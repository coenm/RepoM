namespace RepoM.App.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using RepoM.Api.Git;

public partial class RepositoryComponent : UserControl
{
    // Delay before the context menu is prefetched in the background while hovering a repository.
    private static readonly TimeSpan _defaultHoverPrefetchDelay = TimeSpan.FromSeconds(2);
    private readonly DispatcherTimer _hoverTimer;
    private RepositoryList? _ownerList;

    public RepositoryComponent()
    {
        InitializeComponent();

        _hoverTimer = new DispatcherTimer { Interval = _defaultHoverPrefetchDelay, };
        _hoverTimer.Tick += HoverTimer_Tick;

        Unloaded += (_, _) => _hoverTimer.Stop();
        DataContextChanged += (_, _) => _hoverTimer.Stop();
    }

    private void Root_MouseEnter(object sender, MouseEventArgs e)
    {
        _ownerList = FindAncestor<RepositoryList>(this);
        TimeSpan delay = _ownerList?.MenuPrefetchHoverDelay ?? _defaultHoverPrefetchDelay;

        _hoverTimer.Stop();

        if (delay <= TimeSpan.Zero)
        {
            // Prefetching disabled via configuration.
            return;
        }

        _hoverTimer.Interval = delay;
        _hoverTimer.Start();
    }

    private void Root_MouseLeave(object sender, MouseEventArgs e)
    {
        _hoverTimer.Stop();
    }

    private void HoverTimer_Tick(object? sender, EventArgs e)
    {
        _hoverTimer.Stop();

        if (DataContext is not RepositoryViewModel vm)
        {
            return;
        }

        (_ownerList ?? FindAncestor<RepositoryList>(this))?.PrefetchContextMenu(vm);
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        for (DependencyObject? parent = VisualTreeHelper.GetParent(current); parent != null; parent = VisualTreeHelper.GetParent(parent))
        {
            if (parent is T typed)
            {
                return typed;
            }
        }

        return null;
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