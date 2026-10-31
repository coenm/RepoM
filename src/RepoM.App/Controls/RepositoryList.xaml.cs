namespace RepoM.App.Controls;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RepoM.Api.Git;
using RepoM.App.ViewModels;
using WpfContextMenu = System.Windows.Controls.ContextMenu;

public partial class RepositoryList : UserControl
{
    private CancellationTokenSource? _contextMenuCts;

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

    // Hover delay before prefetching; TimeSpan.Zero (or less) disables hover prefetching.
    internal TimeSpan MenuPrefetchHoverDelay => ViewModel.MenuPrefetchHoverDelay;

    internal void FocusList()
    {
        lstRepositories.Focus();
    }

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
        if (e.OriginalSource is not (Grid or TextBlock))
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

    private async void LstRepositories_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
    {
        if (sender == null)
        {
            e.Handled = true;
            return;
        }

        WpfContextMenu ctxMenu = ((FrameworkElement)e.Source).ContextMenu!;
        var listContextMenuOpening = await LstRepositoriesContextMenuOpeningWrapperAsync(ctxMenu).ConfigureAwait(true);
        if (!listContextMenuOpening)
        {
            e.Handled = true;
        }
    }

    private async Task<bool> LstRepositoriesContextMenuOpeningWrapperAsync(WpfContextMenu ctxMenu)
    {
        try
        {
            return await LstRepositoriesContextMenuOpeningAsync(ctxMenu).ConfigureAwait(true);
        }
        catch (Exception e)
        {
            ViewModel.LogContextMenuError(e);

            ctxMenu.Items.Clear();
            ctxMenu.Items.Add(new AcrylicMenuItem
            {
                Header = "Error",
                IsEnabled = false,
            });
            ctxMenu.Items.Add(new AcrylicMenuItem
            {
                Header = e.Message,
                IsEnabled = false,
            });

            return false;
        }
    }

    private async Task<bool> LstRepositoriesContextMenuOpeningAsync(WpfContextMenu ctxMenu)
    {
        // A newer open request supersedes this one, so a slow older build cannot overwrite a newer menu.
        _contextMenuCts?.Cancel();
        var cts = new CancellationTokenSource();
        _contextMenuCts = cts;

        // WPF opens the menu as soon as the handler yields, so never leave the previous repository's items in it.
        ctxMenu.Items.Clear();
        ctxMenu.Items.Add(new AcrylicMenuItem
        {
            Header = "Loading...",
            IsEnabled = false,
        });

        try
        {
            var entries = await ViewModel.CreateContextMenuEntriesAsync(cts.Token).ConfigureAwait(true);
            if (cts.IsCancellationRequested)
            {
                return false;
            }

            if (entries.Count == 0)
            {
                ctxMenu.Items.Clear();
                ctxMenu.IsOpen = false;
                return false;
            }

            ctxMenu.Items.Clear();
            PopulateMenuItems(ctxMenu.Items, entries);
            return true;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
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

    private void PopulateMenuItems(ItemCollection items, System.Collections.Generic.IReadOnlyList<IRepositoryMenuEntryViewModel> entries)
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
            item.Items.Add(new Separator());
            item.SetSubMenuOpened(async (_, _) =>
            {
                item.ClearSubMenuOpened();
                try
                {
                    var childEntries = await itemViewModel.LoadChildrenAsync().ConfigureAwait(true);
                    item.ClearItems();
                    PopulateMenuItems(item.Items, childEntries);
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

            var listContextMenuOpening = await LstRepositoriesContextMenuOpeningWrapperAsync(ctxMenu).ConfigureAwait(true);
            if (listContextMenuOpening)
            {
                ctxMenu.Placement = PlacementMode.Left;
                ctxMenu.PlacementTarget = (UIElement)e.OriginalSource;
                ctxMenu.IsOpen = true;
            }
        }
    }
}