namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.RepositoryActions;
using RepoM.App.Controls;
using RepoM.App.Plugins;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryOrdering;
using RepoM.App.Services;
using RepoM.App.ViewModels;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using SourceChord.FluentWPF;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private volatile bool _refreshDelayed;
    private DateTime _timeOfLastRefresh = DateTime.MinValue;
    private bool _closeOnDeactivate = true;
    private readonly IRepositoryIgnoreStore _repositoryIgnoreStore;
    private readonly DefaultRepositoryMonitor? _monitor;
    private readonly ITranslationService _translationService;
    private readonly IFileSystem _fileSystem;
    private readonly ActionExecutor _executor;
    private readonly IRepositoryFilteringManager _repositoryFilteringManager;
    private readonly IRepositoryMatcher _repositoryMatcher;
    private readonly ILogger _logger;
    private readonly IUserMenuActionMenuFactory _userMenuActionFactory;
    private readonly IAppDataPathProvider _appDataPathProvider;

    private readonly List<Separator> _separators = new();
    private readonly List<AcrylicMenuItem> _menuItems = new();
    private int _separatorIndex = 0;
    private int _menuItemIndex = 0;

    private readonly AcrylicMenuItem _loadingMenuItem = new()
        {
            Header = "Loading ..",
            IsEnabled = true,
        };

    public MainWindow(
        IRepositoryInformationAggregator aggregator,
        IRepositoryMonitor repositoryMonitor,
        IRepositoryIgnoreStore repositoryIgnoreStore,
        IAppSettingsService appSettingsService,
        ITranslationService translationService,
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        ActionExecutor executor,
        IRepositoryComparerManager repositoryComparerManager,
        IThreadDispatcher threadDispatcher,
        IRepositoryFilteringManager repositoryFilteringManager,
        IRepositoryMatcher repositoryMatcher,
        IModuleManager moduleManager,
        ILogger logger,
        IUserMenuActionMenuFactory userMenuActionFactory)
    {
        _repositoryFilteringManager = repositoryFilteringManager ?? throw new ArgumentNullException(nameof(repositoryFilteringManager));
        _repositoryMatcher = repositoryMatcher ?? throw new ArgumentNullException(nameof(repositoryMatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userMenuActionFactory = userMenuActionFactory ?? throw new ArgumentNullException(nameof(userMenuActionFactory));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryIgnoreStore = repositoryIgnoreStore ?? throw new ArgumentNullException(nameof(repositoryIgnoreStore));
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));

        InitializeComponent();

        SetAcrylicWindowStyle(this, AcrylicWindowStyle.None);
        
        var orderingsViewModel = new OrderingsViewModel(repositoryComparerManager, threadDispatcher);
        var queryParsersViewModel = new QueryParsersViewModel(_repositoryFilteringManager, threadDispatcher);
        var filterViewModel = new FiltersViewModel(_repositoryFilteringManager, threadDispatcher);
        var pluginsViewModel = new PluginCollectionViewModel(moduleManager);

        DataContext = new MainWindowViewModel(
            appSettingsService,
            orderingsViewModel,
            queryParsersViewModel,
            filterViewModel,
            pluginsViewModel,
            new HelpViewModel(_translationService));
        SettingsMenu.DataContext = DataContext; // this is out of the visual tree

        _monitor = repositoryMonitor as DefaultRepositoryMonitor;
        if (_monitor != null)
        {
            _monitor.OnScanStateChanged += OnScanStateChanged;
            ShowScanningState(_monitor.Scanning);
        }
        
        lstRepositories.ItemsSource = aggregator.Repositories;

        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(aggregator.Repositories);
        ((ICollectionView)view).CollectionChanged += View_CollectionChanged;
        view.Filter = FilterRepositories;
        view.CustomSort = repositoryComparerManager.Comparer;
        repositoryComparerManager.SelectedRepositoryComparerKeyChanged += (_, _) => view.Refresh();
        repositoryFilteringManager.SelectedQueryParserChanged += (_, _) => view.Refresh();
        repositoryFilteringManager.SelectedFilterChanged += (_, _) => view.Refresh();

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

    private async void LstRepositories_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        // prevent doubleclicks from scrollbars and other non-data areas
        if (e.OriginalSource is not (Grid or TextBlock))
        {
            return;
        }

        try
        {
            await InvokeActionOnCurrentRepositoryAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not invoke action on current repository.");
        }
    }

    private async void LstRepositories_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
    {
        if (sender == null)
        {
            e.Handled = true;
            return;
        }

        // var currentCursor = ((FrameworkElement)e.Source).Cursor;
        // ((FrameworkElement)e.Source).Cursor = Cursors.AppStarting;
        var lstRepositoriesContextMenuOpening = await LstRepositoriesContextMenuOpeningWrapperAsync(((FrameworkElement)e.Source).ContextMenu).ConfigureAwait(true);
        if (!lstRepositoriesContextMenuOpening)
        {
            e.Handled = true;
        }
    }
    
    private async Task<bool> LstRepositoriesContextMenuOpeningWrapperAsync(ContextMenu ctxMenu)
    {
        try
        {
            return await LstRepositoriesContextMenuOpeningAsync(ctxMenu).ConfigureAwait(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not create menu.");

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

    private async Task<bool> LstRepositoriesContextMenuOpeningAsync(ContextMenu ctxMenu)
    {
        ResetIndex();
        if (lstRepositories.SelectedItem is not RepositoryViewModel vm)
        {
            return false;
        }

        var items = new List<Control>();
        // ItemCollection items = ctxMenu.Items;
        // ItemCollection items = new ItemCollection();
        //items.Clear();

        // foreach (var item in ctxMenu.Items)
        // {
        //     if (item is Control c)
        //     {
        //         c.IsEnabled = false;
        //     }
        // }

        ctxMenu.Items.Clear();
        ctxMenu.Items.Add(_loadingMenuItem);

        await foreach (UserInterfaceRepositoryActionBase action in _userMenuActionFactory.CreateMenuAsync(vm.Repository).ConfigureAwait(true))
        {
            if (action is UserInterfaceSeparatorRepositoryAction)
            {
                if (items.Count > 0 && items[^1] is not Separator)
                {
                    items.Add(GetSeparator());
                }
            }
            else if (action is DeferredSubActionsUserInterfaceRepositoryAction or UserInterfaceRepositoryAction)
            {
                Control? controlItem = CreateMenuItemNewStyleAsync(action, vm);
                if (controlItem != null)
                {
                    items.Add(controlItem);
                }
            }
        }

        ctxMenu.Items.Clear();
        foreach (Control item in items)
        {
            ctxMenu.Items.Add(item);
        }

        return true;
    }
    

    private async void LstRepositories_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Return or Key.Enter)
        {
            try
            {
                await InvokeActionOnCurrentRepositoryAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

            // try open context menu.
            ContextMenu? ctxMenu = ((FrameworkElement)e.Source).ContextMenu;
            if (ctxMenu == null)
            {
                return;
            }

            var lstRepositoriesContextMenuOpening = await LstRepositoriesContextMenuOpeningWrapperAsync(ctxMenu).ConfigureAwait(true);
            if (lstRepositoriesContextMenuOpening)
            {
                ctxMenu.Placement = PlacementMode.Left;
                ctxMenu.PlacementTarget = (UIElement)e.OriginalSource;
                ctxMenu.IsOpen = true;
            }
        }
    }
   
    private async Task InvokeActionOnCurrentRepositoryAsync()
    {
        if (lstRepositories.SelectedItem is not RepositoryViewModel selectedView)
        {
            return;
        }

        if (!selectedView.WasFound)
        {
            return;
        }

        var skip = 0;
        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            skip = 1;
        }

        UserInterfaceRepositoryActionBase uiRepositoryAction = await _userMenuActionFactory
            .CreateMenuAsync(selectedView.Repository)
            .Skip(skip)
            .FirstAsync()
            .ConfigureAwait(false);

        if (uiRepositoryAction is not UserInterfaceRepositoryAction action)
        {
            return;
        }

        if (action.RepositoryCommand is NullRepositoryCommand)
        {
            return;
        }

        _executor.Execute(action.Repository, action.RepositoryCommand);
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
        var directoryName = _appDataPathProvider.AppDataPath;

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
        Point position = GetTopLeftPlaceFormByTaskBarLocation(
            SystemParameters.WorkArea,
            Height,
            Width,
            Screen.PrimaryScreen);
        Left = position.X;
        Top = position.Y;
    }

    private static Point GetTopLeftPlaceFormByTaskBarLocation(Rect workArea, double height, double width, Screen? primaryScreen)
    {
        var topY = workArea.Top;
        var bottomY = workArea.Height - height;
        var leftX = workArea.Left;
        var rightX = workArea.Width - width;

        return TaskBarLocator.GetTaskBarLocation(primaryScreen) switch
            {
                TaskBarLocator.TaskBarLocation.Top => new Point(rightX, topY),
                TaskBarLocator.TaskBarLocation.Left => new Point(leftX, bottomY),
                TaskBarLocator.TaskBarLocation.Bottom or TaskBarLocator.TaskBarLocation.Right => new Point(rightX, bottomY),
                _ => new Point(rightX, bottomY),
            };
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

    private Control? /*MenuItem*/ CreateMenuItem(RepositoryActionBase action, RepositoryViewModel? affectedViews = null)
    {
        if (action is RepositorySeparatorAction)
        {
            return GetSeparator();
        }

        if (action is not RepositoryAction repositoryAction)
        {
            // throw??
            return null;
        }

        Action<object, object> clickAction = (object clickSender, object clickArgs) =>
            {
                if (repositoryAction?.Action is null or NullRepositoryCommand)
                {
                    return;
                }
                
                // run actions in the UI async to not block it
                if (repositoryAction.ExecutionCausesSynchronizing)
                {
                    Task.Run(() => SetVmSynchronizing(affectedViews, true))
                        .ContinueWith(t => _executor.Execute(action.Repository, action.Action))
                        .ContinueWith(t => SetVmSynchronizing(affectedViews, false));
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
        if (repositoryAction is DeferredSubActionsRepositoryAction deferredRepositoryAction && deferredRepositoryAction.DeferredSubActionsEnumerator != null)
        {
            // this is a template submenu item to enable submenus under the current
            // menu item. this item gets removed when the real subitems are created
            item.Items.Add(string.Empty);

            void SelfDetachingEventHandler(object _, RoutedEventArgs evtArgs)
            {
                item.SubmenuOpened -= SelfDetachingEventHandler;
                item.Items.Clear();

                foreach (RepositoryActionBase subAction in deferredRepositoryAction.DeferredSubActionsEnumerator())
                {
                    Control? controlItem = CreateMenuItem(subAction);
                    if (controlItem != null)
                    {
                        item.Items.Add(controlItem);
                    }
                }
            }

            item.SubmenuOpened += SelfDetachingEventHandler;
        }
        else if (repositoryAction.SubActions != null)
        {
            foreach (RepositoryActionBase subAction in repositoryAction.SubActions)
            {
                Control? controlItem = CreateMenuItem(subAction);
                if (controlItem != null)
                {
                    item.Items.Add(controlItem);
                }
            }
        }

        return item;
    }

    private Control? /*MenuItem*/ CreateMenuItemNewStyleAsync(UserInterfaceRepositoryActionBase action, RepositoryViewModel? affectedViews = null)
    {
        if (action is UserInterfaceSeparatorRepositoryAction)
        {
            return GetSeparator();
        }

        // UserInterfaceRepositoryAction
        // DeferredSubActionsUserInterfaceRepositoryAction

        if (action is not UserInterfaceRepositoryAction repositoryAction)
        {
            // throw??
            return null;
        }

        Action<object, object> clickAction = (object clickSender, object clickArgs) =>
        {
            if (repositoryAction.RepositoryCommand is null or NullRepositoryCommand)
            {
                return;
            }

            // run actions in the UI async to not block it
            if (repositoryAction.ExecutionCausesSynchronizing)
            {
                Task.Run(() => SetVmSynchronizing(affectedViews, true))
                    .ContinueWith(t => _executor.Execute(action.Repository, action.RepositoryCommand))
                    .ContinueWith(t => SetVmSynchronizing(affectedViews, false));
            }
            else
            {
                Task.Run(() => _executor.Execute(action.Repository, action.RepositoryCommand));
            }
        };

        AcrylicMenuItem item = GetMenuItem();
        item.Header = repositoryAction.Name;
        item.IsEnabled = repositoryAction.CanExecute;
        item.Items.Clear();
        item.Click += new RoutedEventHandler(clickAction);

        // this is a deferred submenu. We want to make sure that the context menu can pop up
        // fast, while submenus are not evaluated yet. We don't want to make the context menu
        // itself slow because the creation of the submenu items takes some time.
        if (repositoryAction is DeferredSubActionsUserInterfaceRepositoryAction deferredRepositoryAction)
        {
            // this is a template submenu item to enable submenus under the current
            // menu item. this item gets removed when the real subitems are created
            item.Items.Add("Loading..");

            async void SelfDetachingEventHandler(object _, RoutedEventArgs evtArgs)
            {
                item.SubmenuOpened -= SelfDetachingEventHandler;
                item.Items.Clear();
                
                foreach (UserInterfaceRepositoryActionBase subAction in await deferredRepositoryAction.GetAsync().ConfigureAwait(true))
                {
                    Control? controlItem = CreateMenuItemNewStyleAsync(subAction);
                    if (controlItem == null)
                    {
                        continue;
                    }

                    if (controlItem is not Separator)
                    {
                        item.Items.Add(controlItem);
                        continue;
                    }

                    if (item.Items.Count > 0 && item.Items[^1] is not Separator)
                    {
                        item.Items.Add(controlItem);
                    }
                }

                var count = item.Items.Count;
                if (count > 0 && item.Items[^1] is Separator)
                {
                    item.Items.RemoveAt(count - 1);
                }
            }

            item.SubmenuOpened += SelfDetachingEventHandler;
        }
        else if (repositoryAction.SubActions != null)
        {
            // this is a template submenu item to enable submenus under the current
            // menu item. this item gets removed when the real subitems are created
            item.Items.Add("Loading..");
            
            async void SelfDetachingEventHandler1(object _, RoutedEventArgs evtArgs)
            {
                item.SubmenuOpened -= SelfDetachingEventHandler1;
                item.Items.Clear();

                foreach (UserInterfaceRepositoryActionBase subAction in repositoryAction.SubActions)
                {
                    Control? controlItem = CreateMenuItemNewStyleAsync(subAction);
                    if (controlItem == null)
                    {
                        continue;
                    }

                    if (controlItem is not Separator)
                    {
                        item.Items.Add(controlItem);
                        continue;
                    }

                    if (item.Items.Count > 0 && item.Items[^1] is not Separator)
                    {
                        item.Items.Add(controlItem);
                    }
                }

                var count = item.Items.Count;
                if (count > 0 && item.Items[^1] is Separator)
                {
                    item.Items.RemoveAt(count - 1);
                }
            }

            item.SubmenuOpened += SelfDetachingEventHandler1;
        }

        return item;
    }

    private static void SetVmSynchronizing(RepositoryViewModel? affectedVm, bool synchronizing)
    {
        if (affectedVm != null)
        {
            affectedVm.IsSynchronizing = synchronizing;
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
            AcrylicWindowStyle currentStyle = GetAcrylicWindowStyle(this);
            AcrylicWindowStyle newStyle = currentStyle == AcrylicWindowStyle.None
                ? AcrylicWindowStyle.Normal
                : AcrylicWindowStyle.None;
            SetAcrylicWindowStyle(this, newStyle);
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
            _timeOfLastRefresh = DateTime.UtcNow;
        }

        // Spin while updates are in progress
        if (DateTime.UtcNow - _timeOfLastRefresh < TimeSpan.FromMilliseconds(100))
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
        var query = txtFilter.Text.Trim();

        if (_refreshDelayed)
        {
            return false;
        }

        if (item is not RepositoryViewModel viewModelItem)
        {
            return false;
        }

        try
        {
            IQuery? alwaysVisibleFilter = _repositoryFilteringManager.AlwaysVisibleFilter;
            if (alwaysVisibleFilter != null && _repositoryMatcher.Matches(viewModelItem.Repository, alwaysVisibleFilter))
            {
                return true;
            }
        }
        catch (Exception)
        {
            return false;
        }

        try
        {
            if (!_repositoryMatcher.Matches(viewModelItem.Repository, _repositoryFilteringManager.PreFilter))
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        if (_refreshDelayed)
        {
            return false;
        }

        try
        {
            IQuery queryObject = _repositoryFilteringManager.QueryParser.Parse(query);
            return _repositoryMatcher.Matches(viewModelItem.Repository, queryObject);
        }
        catch (Exception)
        {
            return false;
        }
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
    
    private void ResetIndex()
    {
        _separatorIndex = 0;
        _menuItemIndex = 0;
    }

    private Separator GetSeparator()
    {
        while (_separatorIndex >= _separators.Count)
        {
            _separators.Add(new Separator());
        }

        return _separators[_separatorIndex++];
    }

    private AcrylicMenuItem GetMenuItem()
    {
        while (_menuItemIndex >= _menuItems.Count)
        {
            _menuItems.Add(new AcrylicMenuItem());
        }

        return _menuItems[_menuItemIndex++];
    }

    public bool IsShown => Visibility == Visibility.Visible && IsActive;
}