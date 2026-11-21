namespace RepoM.UI.Controls;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using RepoM.Api.Git;
using RepoM.UI.ViewModels;
using WpfContextMenu = System.Windows.Controls.ContextMenu;

public partial class RepositoryList : UserControl
{
    private CancellationTokenSource? _contextMenuCts;

    internal int ContextMenuLoadingIndicatorDelayMilliseconds { get; set; } = 200;

    public RepositoryList()
    {
        InitializeComponent();
    }

    internal void Initialize(RepositoryListViewModel viewModel)
    {
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    // Triggered after the user hovers a repository; builds its context menu in the background.
    internal void PrefetchContextMenu(RepositoryViewModel repository)
    {
        ViewModel.StartContextMenuPrefetch(repository);
    }

    // Zero starts immediately; a negative delay disables hover prefetching.
    internal TimeSpan MenuPrefetchHoverDelay => ViewModel.MenuPrefetchHoverDelay;

    internal void FocusList()
    {
        lstRepositories.Focus();
    }

    internal void CloseContextMenu() => mnuRepositoryContext.IsOpen = false;

    internal void FocusFirstItem()
    {
        lstRepositories.Focus();
        if (lstRepositories.Items.Count <= 0)
        {
            return;
        }

        lstRepositories.SelectedIndex = 0;
        if (lstRepositories.ItemContainerGenerator.ContainerFromIndex(0) is ListBoxItem item)
        {
            item.Focus();
        }
    }

    private RepositoryListViewModel ViewModel => DataContext as RepositoryListViewModel
        ?? throw new InvalidOperationException("RepositoryList is not initialized.");

    private async void LstRepositories_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not (Grid or TextBlock) || IsInsideButton(e.OriginalSource))
        {
            return;
        }

        try
        {
            await ViewModel.InvokeDefaultActionOnSelectionAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            ViewModel.LogInvokeActionError(exception);
        }
    }

    private void LstRepositories_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
    {
        if (sender == null || e.Source is not FrameworkElement target || target.ContextMenu is not WpfContextMenu ctxMenu)
        {
            e.Handled = true;
            return;
        }

        e.Handled = true;
        _ = OpenContextMenuAsync(ctxMenu, target, PlacementMode.MousePoint);
    }

    private async Task<bool> OpenContextMenuAsync(WpfContextMenu ctxMenu, UIElement placementTarget, PlacementMode placement)
    {
        Window window = Window.GetWindow(this);
        if (!ctxMenu.Resources.MergedDictionaries.Contains(window.Resources))
        {
            ctxMenu.Resources.MergedDictionaries.Add(window.Resources);
        }

        _contextMenuCts?.Cancel();
        ctxMenu.IsOpen = false;
        var cts = new CancellationTokenSource();
        _contextMenuCts = cts;

        try
        {
            string repositoryPath = ViewModel.SelectedRepository?.Path ?? string.Empty;
            var timer = Stopwatch.StartNew();
            ctxMenu.Items.Clear();
            ctxMenu.Placement = placement;
            ctxMenu.PlacementTarget = placementTarget;

            var entriesTask = ViewModel.CreateContextMenuEntriesAsync(cts.Token);
            var menuOpenedForLoading = await WaitForContextMenuLoadingIndicatorAsync(
                entriesTask, ContextMenuLoadingIndicatorDelayMilliseconds, cts.Token).ConfigureAwait(true);
            if (menuOpenedForLoading)
            {
                cts.Token.ThrowIfCancellationRequested();
                ctxMenu.Items.Add(new AcrylicMenuItem { Header = "Loading...", IsEnabled = false });
                ctxMenu.IsOpen = true;
            }

            var entries = await entriesTask.ConfigureAwait(true);
            TimeSpan wait = timer.Elapsed;
            cts.Token.ThrowIfCancellationRequested();
            if (entries.Count == 0)
            {
                ctxMenu.Items.Clear();
                ctxMenu.IsOpen = false;
                return false;
            }

            ctxMenu.Items.Clear();
            timer.Restart();
            PopulateMenuItems(ctxMenu.Items, entries);
            ViewModel.LogMenuPresentation(repositoryPath, "root", wait, timer.Elapsed);
            if (!menuOpenedForLoading)
            {
                ctxMenu.IsOpen = true;
            }

            return true;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            if (cts.IsCancellationRequested || !ReferenceEquals(_contextMenuCts, cts))
            {
                return false;
            }

            ViewModel.LogContextMenuError(exception);
            ctxMenu.Items.Clear();
            ctxMenu.Items.Add(new AcrylicMenuItem { Header = "Error", IsEnabled = false });
            ctxMenu.Items.Add(new AcrylicMenuItem { Header = exception.Message, IsEnabled = false });
            ctxMenu.IsOpen = true;
            return false;
        }
        finally
        {
            if (ReferenceEquals(_contextMenuCts, cts))
            {
                _contextMenuCts = null;
            }

            cts.Dispose();
        }
    }

    internal static async Task<bool> WaitForContextMenuLoadingIndicatorAsync(
        Task entriesTask, int delayMilliseconds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entriesTask);
        cancellationToken.ThrowIfCancellationRequested();
        if (delayMilliseconds < 0)
        {
            return false;
        }

        using var delayCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var delayTask = Task.Delay(delayMilliseconds, delayCts.Token);
        Task first = await Task.WhenAny(entriesTask, delayTask).ConfigureAwait(true);
        if (first == delayTask)
        {
            await delayTask.ConfigureAwait(true);
            return true;
        }

        delayCts.Cancel();
        return false;
    }

    internal void PopulateMenuItems(ItemCollection items, System.Collections.Generic.IReadOnlyList<IRepositoryMenuEntryViewModel> entries)
    {
        foreach (IRepositoryMenuEntryViewModel entry in entries)
        {
            if (entry is RepositoryMenuSeparatorViewModel)
            {
                if (items.Count == 0 || items[^1] is Separator)
                {
                    continue;
                }

                items.Add(new Separator());
                continue;
            }

            if (entry is RepositoryMenuItemViewModel itemViewModel)
            {
                items.Add(CreateMenuItem(itemViewModel));
            }
        }

        if (items.Count > 0 && items[^1] is Separator)
        {
            items.RemoveAt(items.Count - 1);
        }
    }

    private AcrylicMenuItem CreateMenuItem(RepositoryMenuItemViewModel itemViewModel)
    {
        var item = new AcrylicMenuItem();
        item.SetHeader(itemViewModel.Header);
        item.SetEnabled(itemViewModel.IsEnabled);

        if (itemViewModel.HasSubItems)
        {
            item.Items.Add(new AcrylicMenuItem { Header = "Loading...", IsEnabled = false });
            string repositoryPath = ViewModel.SelectedRepository?.Path ?? string.Empty;
            item.SetSubMenuOpened(async (_, _) =>
            {
                item.ClearSubMenuOpened();
                try
                {
                    var timer = Stopwatch.StartNew();
                    var childEntries = await itemViewModel.LoadChildrenAsync().ConfigureAwait(true);
                    TimeSpan wait = timer.Elapsed;
                    item.ClearItems();
                    timer.Restart();
                    PopulateMenuItems(item.Items, childEntries);
                    ViewModel.LogMenuPresentation(repositoryPath, itemViewModel.Header, wait, timer.Elapsed);
                }
                catch (Exception exception)
                {
                    ViewModel.LogContextMenuError(exception);
                    item.ClearItems();
                    item.Items.Add(new AcrylicMenuItem
                    {
                        Header = "Error",
                        IsEnabled = false,
                    });
                    item.Items.Add(new AcrylicMenuItem
                    {
                        Header = exception.Message,
                        IsEnabled = false,
                    });
                }
            });
        }
        else
        {
            item.SetClick((_, _) => itemViewModel.Execute());
        }

        return item;
    }

    private async void LstRepositories_KeyDown(object? sender, KeyEventArgs e)
    {
        if (IsInsideButton(e.OriginalSource))
        {
            return;
        }

        if (e.Key is Key.Return or Key.Enter)
        {
            try
            {
                await ViewModel.InvokeDefaultActionOnSelectionAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                ViewModel.LogInvokeActionError(exception);
            }

            return;
        }

        if (e.Key is Key.Left or Key.Right)
        {
            if (sender == null)
            {
                e.Handled = true;
                return;
            }

            WpfContextMenu? ctxMenu = ((FrameworkElement)e.Source).ContextMenu;
            if (ctxMenu == null)
            {
                e.Handled = true;
                return;
            }

            e.Handled = true;
            await OpenContextMenuAsync(ctxMenu, (UIElement)e.OriginalSource, PlacementMode.Left).ConfigureAwait(true);

        }
    }

    private static bool IsInsideButton(object source)
    {
        for (DependencyObject? element = source as DependencyObject; element is Visual; element = VisualTreeHelper.GetParent(element))
        {
            if (element is ButtonBase)
            {
                return true;
            }
        }

        return false;
    }
}