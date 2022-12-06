namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using RepoM.Api;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.App.Controls;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryOrdering;
using RepoM.App.Services;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using SourceChord.FluentWPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly IRepositoryActionProvider _repositoryActionProvider;
    private readonly IRepositoryIgnoreStore _repositoryIgnoreStore;
    private readonly DefaultRepositoryMonitor? _monitor;
    private readonly ITranslationService _translationService;
    private readonly IRepositorySearch _repositorySearch;
    private bool _closeOnDeactivate = true;
    private bool _refreshDelayed;
    private DateTime _timeOfLastRefresh = DateTime.MinValue;
    private readonly IFileSystem _fileSystem;
    private readonly ActionExecutor _executor;
    private readonly IAppDataPathProvider _appDataPathProvider;

    public MainWindow(
        StatusCharacterMap statusCharacterMap,
        IRepositoryInformationAggregator aggregator,
        IRepositoryMonitor repositoryMonitor,
        IRepositoryActionProvider repositoryActionProvider,
        IRepositoryIgnoreStore repositoryIgnoreStore,
        IAppSettingsService appSettingsService,
        ITranslationService translationService,
        IAppDataPathProvider appDataPathProvider,
        IRepositorySearch repositorySearch,
        IFileSystem fileSystem,
        ActionExecutor executor,
        IRepositoryComparerManager repositoryComparerManager,
        IThreadDispatcher threadDispatcher)
    {
        _translationService = translationService;
        InitializeComponent();

        AcrylicWindow.SetAcrylicWindowStyle(this, AcrylicWindowStyle.None);

        var orderingsViewModel = new OrderingsViewModel(repositoryComparerManager, threadDispatcher);
        DataContext = new MainWindowPageModel(appSettingsService, orderingsViewModel);
        SettingsMenu.DataContext = DataContext; // this is out of the visual tree

        _monitor = repositoryMonitor as DefaultRepositoryMonitor;
        if (_monitor != null)
        {
            _monitor.OnScanStateChanged += OnScanStateChanged;
            ShowScanningState(_monitor.Scanning);
        }

        _repositoryActionProvider = repositoryActionProvider ?? throw new ArgumentNullException(nameof(repositoryActionProvider));
        _repositoryIgnoreStore = repositoryIgnoreStore ?? throw new ArgumentNullException(nameof(repositoryIgnoreStore));
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _repositorySearch = repositorySearch ?? throw new ArgumentNullException(nameof(repositorySearch));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));

        lstRepositories.ItemsSource = aggregator.Repositories;

        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(aggregator.Repositories);
        ((ICollectionView)view).CollectionChanged += View_CollectionChanged;
        view.Filter = FilterRepositories;
        view.CustomSort = repositoryComparerManager.Comparer;
        repositoryComparerManager.SelectedRepositoryComparerKeyChanged += (_, _) => view.Refresh();
        
        AssemblyName? appName = Assembly.GetEntryAssembly()?.GetName();
        txtHelpCaption.Text = appName?.Name + " " + appName?.Version?.ToString(2);
        txtHelp.Text = GetHelp(statusCharacterMap);

        PlaceFormByTaskBarLocation();
    }

    private void View_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // use the list's itemsource directly, this one is not filtered (otherwise searching in the UI without matches could lead to the "no repositories yet"-screen)
        var hasRepositories = lstRepositories.ItemsSource.OfType<RepositoryViewModel>().Any();
        tbNoRepositories.Visibility = hasRepositories ? Visibility.Hidden : Visibility.Visible;
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        ShowUpdateIfAvailable();
        txtFilter.Focus();
        txtFilter.SelectAll();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        if (_closeOnDeactivate)
        {
            Hide();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key != Key.Escape)
        {
            return;
        }

        var isFilterActive = txtFilter.IsFocused && !string.IsNullOrEmpty(txtFilter.Text);
        if (!isFilterActive)
        {
            Hide();
        }
    }

    public void ShowAndActivate()
    {
        Dispatcher.Invoke(() =>
            {
                PlaceFormByTaskBarLocation();

                if (!IsShown)
                {
                    Show();
                }

                Activate();
                txtFilter.Focus();
                txtFilter.SelectAll();
            });
    }

    private void OnScanStateChanged(object? sender, bool isScanning)
    {
        Dispatcher.Invoke(() => ShowScanningState(isScanning));
    }

    private void LstRepositories_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        // prevent doubleclicks from scrollbars and other non-data areas
        if (e.OriginalSource is Grid or TextBlock)
        {
            InvokeActionOnCurrentRepository();
        }
    }

    private void LstRepositories_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
    {
        if (sender == null)
        {
            e.Handled = true;
            return;
        }

        if (!LstRepositoriesContextMenuOpening(sender, ((FrameworkElement)e.Source).ContextMenu))
        {
            e.Handled = true;
        }
    }

    private bool LstRepositoriesContextMenuOpening(object sender, ContextMenu ctxMenu)
    {
        RepositoryViewModel[] selectedViews = lstRepositories.SelectedItems.OfType<RepositoryViewModel>().ToArray();

        if (!selectedViews.Any())
        {
            return false;
        }

        ItemCollection items = ctxMenu.Items;
        items.Clear();

        IEnumerable<Repository> innerRepositories = selectedViews.Select(view => view.Repository);
        foreach (RepositoryActionBase action in _repositoryActionProvider.GetContextMenuActions(innerRepositories))
        {
            if (action is RepositorySeparatorAction)
            {
                if (items.Count > 0)
                {
                    items.Add(new Separator());
                }
            }
            else if (action is RepositoryAction _)
            {
                Control? controlItem = CreateMenuItem(sender, action, selectedViews);
                if (controlItem != null)
                {
                    items.Add(controlItem);
                }
            }
            else
            {
                // do nothing.
                // log?
            }
        }

        return true;
    }

    private void LstRepositories_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Return or Key.Enter)
        {
            InvokeActionOnCurrentRepository();
        }
        else if (e.Key is Key.Left or Key.Right)
        {
            if (sender == null)
            {
                e.Handled = true;
                return;
            }

            // try open context menu.
            ContextMenu? ctxMenu = ((FrameworkElement)e.Source).ContextMenu;
            if (ctxMenu != null && LstRepositoriesContextMenuOpening(sender, ctxMenu))
            {
                ctxMenu.Placement = PlacementMode.Left;
                ctxMenu.PlacementTarget = (UIElement)e.OriginalSource;
                ctxMenu.IsOpen = true;
            }
        }
    }

    private void InvokeActionOnCurrentRepository()
    {
        if (lstRepositories.SelectedItem is not RepositoryViewModel selectedView)
        {
            return;
        }

        if (!selectedView.WasFound)
        {
            return;
        }

        RepositoryActionBase? action;

        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            action = _repositoryActionProvider.GetSecondaryAction(selectedView.Repository);
        }
        else
        {
            action = _repositoryActionProvider.GetPrimaryAction(selectedView.Repository);
        }

        if (action != null)
        {
            _executor.Execute(action.Repository, action.Action);
        }
    }

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        transitionerMain.SelectedIndex = transitionerMain.SelectedIndex == 0 ? 1 : 0;
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (MenuButton.ContextMenu != null)
        {
            MenuButton.ContextMenu.IsOpen = true;
        }
    }

    private void ScanButton_Click(object sender, RoutedEventArgs e)
    {
        _monitor?.ScanForLocalRepositoriesAsync();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _monitor?.Reset();
    }

    private void ResetIgnoreRulesButton_Click(object sender, RoutedEventArgs e)
    {
        _repositoryIgnoreStore.Reset();
    }

    private void CustomizeContextMenu_Click(object sender, RoutedEventArgs e)
    {
        var directoryName = _appDataPathProvider.GetAppDataPath();

        if (_fileSystem.Directory.Exists(directoryName))
        {
            Process.Start(new ProcessStartInfo(directoryName)
                {
                    UseShellExecute = true,
                });
        }
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        var hasLink = !string.IsNullOrWhiteSpace(App.AvailableUpdate);
        if (hasLink)
        {
            Navigate(App.AvailableUpdate!);
        }
    }

    private void StarButton_Click(object sender, RoutedEventArgs e)
    {
        Navigate("https://github.com/coenm/RepoM");
    }

    private void FollowButton_Click(object sender, RoutedEventArgs e)
    {
        Navigate("https://twitter.com/Waescher");
    }

    private void SponsorButton_Click(object sender, RoutedEventArgs e)
    {
        Navigate("https://github.com/sponsors/awaescher");
    }

    private static void Navigate(string url)
    {
        Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            });
    }

    private void PlaceFormByTaskBarLocation()
    {
        var topY = SystemParameters.WorkArea.Top;
        var bottomY = SystemParameters.WorkArea.Height - Height;
        var leftX = SystemParameters.WorkArea.Left;
        var rightX = SystemParameters.WorkArea.Width - Width;

        switch (TaskBarLocator.GetTaskBarLocation())
        {
            case TaskBarLocator.TaskBarLocation.Top:
                Top = topY;
                Left = rightX;
                break;
            case TaskBarLocator.TaskBarLocation.Bottom:
                Top = bottomY;
                Left = rightX;
                break;
            case TaskBarLocator.TaskBarLocation.Left:
                Top = bottomY;
                Left = leftX;
                break;
            case TaskBarLocator.TaskBarLocation.Right:
                Top = bottomY;
                Left = rightX;
                break;
        }
    }

    private void ShowUpdateIfAvailable()
    {
        var updateHint = _translationService.Translate("Update hint", App.AvailableUpdate ?? "?.?");

        UpdateButton.Visibility = App.AvailableUpdate == null ? Visibility.Hidden : Visibility.Visible;
        UpdateButton.ToolTip = App.AvailableUpdate == null ? "" : updateHint;
        UpdateButton.Tag = App.AvailableUpdate;

        var parent = (Grid)UpdateButton.Parent;
        parent.ColumnDefinitions[Grid.GetColumn(UpdateButton)].Width = App.AvailableUpdate == null ? new GridLength(0) : GridLength.Auto;
    }

    private Control? /*MenuItem*/ CreateMenuItem(object sender, RepositoryActionBase action, IEnumerable<RepositoryViewModel>? affectedViews = null)
    {
        if (action is RepositorySeparatorAction)
        {
            return new Separator();
        }

        if (action is not RepositoryAction repositoryAction)
        {
            // throw??
            return null;
        }

        Action<object, object> clickAction = (object clickSender, object clickArgs) =>
            {
                if (repositoryAction?.Action == null || repositoryAction.Action is NullAction)
                {
                    return;
                }

                var coords = new float[] { 0, 0, };
                
                // run actions in the UI async to not block it
                if (repositoryAction.ExecutionCausesSynchronizing)
                {
                    Task.Run(() => SetViewsSynchronizing(affectedViews, true))
                        .ContinueWith(t => _executor.Execute(action.Repository, action.Action))
                        .ContinueWith(t => SetViewsSynchronizing(affectedViews, false));
                }
                else
                {
                    Task.Run(() => _executor.Execute(action.Repository, action.Action));
                }
            };

        var item = new AcrylicMenuItem
            {
                Header = repositoryAction.Name,
                IsEnabled = repositoryAction.CanExecute,
            };
        item.Click += new RoutedEventHandler(clickAction);

        // this is a deferred submenu. We want to make sure that the context menu can pop up
        // fast, while submenus are not evaluated yet. We don't want to make the context menu
        // itself slow because the creation of the submenu items takes some time.
        if (repositoryAction?.DeferredSubActionsEnumerator != null)
        {
            // this is a template submenu item to enable submenus under the current
            // menu item. this item gets removed when the real subitems are created
            item.Items.Add(string.Empty);

            void SelfDetachingEventHandler(object _, RoutedEventArgs evtArgs)
            {
                item.SubmenuOpened -= SelfDetachingEventHandler;
                item.Items.Clear();

                foreach (RepositoryActionBase subAction in repositoryAction.DeferredSubActionsEnumerator())
                {
                    Control? controlItem = CreateMenuItem(sender, subAction);
                    if (controlItem != null)
                    {
                        item.Items.Add(controlItem);
                    }
                }

                Console.WriteLine($"Added {item.Items.Count} deferred sub action(s).");
            }

            item.SubmenuOpened += SelfDetachingEventHandler;
        }
        else if (repositoryAction?.SubActions != null)
        {
            foreach (RepositoryActionBase subAction in repositoryAction.SubActions)
            {
                Control? controlItem = CreateMenuItem(sender, subAction);
                if (controlItem != null)
                {
                    item.Items.Add(controlItem);
                }
            }
        }

        return item;
    }

    private static void SetViewsSynchronizing(IEnumerable<RepositoryViewModel>? affectedViews, bool synchronizing)
    {
        if (affectedViews == null)
        {
            return;
        }

        foreach (RepositoryViewModel view in affectedViews)
        {
            view.IsSynchronizing = synchronizing;
        }
    }

    private void ShowScanningState(bool isScanning)
    {
        ScanMenuItem.IsEnabled = !isScanning;
        ScanMenuItem.Header = isScanning
            ? _translationService.Translate("Scanning")
            : _translationService.Translate("ScanComputer");
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.F && Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            txtFilter.Focus();
            txtFilter.SelectAll();
        }

        if (e.Key == Key.Down && txtFilter.IsFocused)
        {
            lstRepositories.Focus();
        }

        // show/hide the titlebar to move the window for screenshots, for example
        if (e.Key == Key.F11)
        {
            AcrylicWindowStyle currentStyle = AcrylicWindow.GetAcrylicWindowStyle(this);
            AcrylicWindowStyle newStyle = currentStyle == AcrylicWindowStyle.None
                ? AcrylicWindowStyle.Normal
                : AcrylicWindowStyle.None;
            AcrylicWindow.SetAcrylicWindowStyle(this, newStyle);
        }

        // keep window open on deactivate to make screenshots, for example
        if (e.Key == Key.F12)
        {
            _closeOnDeactivate = !_closeOnDeactivate;
        }
    }

    private void OnTxtFilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Text has changed, capture the timestamp
        if (sender != null)
        {
            _timeOfLastRefresh = DateTime.Now;
        }

        // Spin while updates are in progress
        if (DateTime.Now - _timeOfLastRefresh < TimeSpan.FromMilliseconds(100))
        {
            if (!_refreshDelayed)
            {
                Dispatcher.InvokeAsync(async () =>
                    {
                        _refreshDelayed = true;
                        await Task.Delay(200);
                        _refreshDelayed = false;
                        OnTxtFilterTextChanged(null, e);
                    });
            }

            return;
        }

        // Refresh the view
        ICollectionView view = CollectionViewSource.GetDefaultView(lstRepositories.ItemsSource);
        view.Refresh();
    }

    private bool FilterRepositories(object item)
    {
        var query = txtFilter.Text;

        if (_refreshDelayed)
        {
            return false;
        }

        if (item is not RepositoryViewModel viewModelItem)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        query = query.Trim();


        if (string.Equals("is:pinned", query, StringComparison.CurrentCultureIgnoreCase))
        {
            return viewModelItem.IsPinned;
        }

        if (string.Equals("is:unpinned", query, StringComparison.CurrentCultureIgnoreCase))
        {
            return !viewModelItem.IsPinned;
        }

        // always show pinned.
        if (viewModelItem.IsPinned)
        {
            // pinned should always be visible
            return true;
        }
        
        if (query.StartsWith("!"))
        {
            var sanitizedQuery = query[1..];
            if (string.IsNullOrWhiteSpace(sanitizedQuery))
            {
                return true;
            }

            var results = _repositorySearch.Search(sanitizedQuery).ToArray();
            return results.Contains(viewModelItem.Path);
        }

        return !_refreshDelayed && viewModelItem.MatchesFilter(txtFilter.Text);
    }

    private void TxtFilter_Finish(object sender, EventArgs e)
    {
        lstRepositories.Focus();
        if (lstRepositories.Items.Count <= 0)
        {
            return;
        }

        lstRepositories.SelectedIndex = 0;
        var item = (ListBoxItem)lstRepositories.ItemContainerGenerator.ContainerFromIndex(0);
        item?.Focus();
    }

    private string GetHelp(StatusCharacterMap statusCharacterMap)
    {
        return _translationService.Translate("Help Detail",
            statusCharacterMap.IdenticalSign,
            statusCharacterMap.StashSign,
            statusCharacterMap.IdenticalSign,
            statusCharacterMap.ArrowUpSign,
            statusCharacterMap.ArrowDownSign,
            statusCharacterMap.NoUpstreamSign,
            statusCharacterMap.StashSign
        );
    }

    public bool IsShown => Visibility == Visibility.Visible && IsActive;
}