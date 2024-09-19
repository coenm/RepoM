namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.App.Plugins;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryOrdering;
using RepoM.App.Services;
using RepoM.App.ViewModels;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = Wpf.Ui.Controls.MenuItem;
using TextBlock = System.Windows.Controls.TextBlock;

[SuppressMessage("ReSharper", "ArrangeAccessorOwnerBody")]
public partial class MainWindow : FluentWindow
{
    private enum IndexNavigator
    {
        GoToNext,
        GoToPrevious,
        GoToFirst,
        GoToLast,
        StickToCurrent,
    }

#pragma warning disable IDE1006
    // ReSharper disable once InconsistentNaming
    private static readonly ImmutableDictionary<Key, IndexNavigator> ListBoxRepos_NavigationKeys = new Dictionary<Key, IndexNavigator>
        {
            { Key.Up, IndexNavigator.GoToPrevious },
            { Key.Down, IndexNavigator.GoToNext },
            { Key.PageUp, IndexNavigator.GoToFirst },
            { Key.PageDown, IndexNavigator.GoToLast },
            { Key.Space, IndexNavigator.GoToNext },
        }.ToImmutableDictionary();
#pragma warning restore IDE1006

    private readonly IAppDataPathProvider        _appDataPathProvider;
    private readonly ActionExecutor              _executor;
    private readonly IFileSystem                 _fileSystem;
    private readonly ILogger                     _logger;
    private readonly DefaultRepositoryMonitor?   _monitor;
    private readonly IRepositoryFilteringManager _repositoryFilteringManager;
    private readonly IRepositoryIgnoreStore      _repositoryIgnoreStore;
    private readonly IRepositoryMatcher          _repositoryMatcher;
    private readonly ITranslationService         _translationService;
    private readonly IUserMenuActionMenuFactory  _userMenuActionFactory;
    private          bool                        _keepMainWindowOpenWhenLosingFocus;
    private volatile bool                        _refreshDelayed;
    private          DateTime                    _timeOfLastRefresh = DateTime.MinValue;

    public MainWindow(IRepositoryInformationAggregator aggregator,
                        IRepositoryMonitor               repositoryMonitor,
                        IRepositoryIgnoreStore           repositoryIgnoreStore,
                        IAppSettingsService              appSettingsService,
                        ITranslationService              translationService,
                        IAppDataPathProvider             appDataPathProvider,
                        IFileSystem                      fileSystem,
                        ActionExecutor                   executor,
                        IRepositoryComparerManager       repositoryComparerManager,
                        IThreadDispatcher                threadDispatcher,
                        IRepositoryFilteringManager      repositoryFilteringManager,
                        IRepositoryMatcher               repositoryMatcher,
                        IModuleManager                   moduleManager,
                        ILogger                          logger,
                        IUserMenuActionMenuFactory       userMenuActionFactory)
    {
        _repositoryFilteringManager = repositoryFilteringManager ?? throw new ArgumentNullException(nameof(repositoryFilteringManager));
        _repositoryMatcher          = repositoryMatcher          ?? throw new ArgumentNullException(nameof(repositoryMatcher));
        _logger                     = logger                     ?? throw new ArgumentNullException(nameof(logger));
        _userMenuActionFactory      = userMenuActionFactory      ?? throw new ArgumentNullException(nameof(userMenuActionFactory));
        _translationService         = translationService         ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryIgnoreStore      = repositoryIgnoreStore      ?? throw new ArgumentNullException(nameof(repositoryIgnoreStore));
        _appDataPathProvider        = appDataPathProvider        ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem                 = fileSystem                 ?? throw new ArgumentNullException(nameof(fileSystem));
        _executor                   = executor                   ?? throw new ArgumentNullException(nameof(executor));

        InitializeComponent();

        var orderingsViewModel    = new OrderingsViewModel(repositoryComparerManager, threadDispatcher);
        var queryParsersViewModel = new QueryParsersViewModel(_repositoryFilteringManager, threadDispatcher);
        var filterViewModel       = new FiltersViewModel(_repositoryFilteringManager, threadDispatcher);
        var pluginsViewModel      = new PluginCollectionViewModel(moduleManager);

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

        ListBoxRepos.ItemsSource = aggregator.Repositories;

        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(aggregator.Repositories);
        ((ICollectionView)view).CollectionChanged                      += View_CollectionChanged;
        view.Filter                                                    =  FilterRepositories;
        view.CustomSort                                                =  repositoryComparerManager.Comparer;
        repositoryComparerManager.SelectedRepositoryComparerKeyChanged += (_, _) => view.Refresh();
        repositoryFilteringManager.SelectedQueryParserChanged          += (_, _) => view.Refresh();
        repositoryFilteringManager.SelectedFilterChanged               += (_, _) => view.Refresh();

        ApplicationThemeManager.ApplySystemTheme(true); // Applies the system theme for Apps, not for
        ApplicationAccentColorManager.ApplySystemAccent();
        WindowBackdrop.ApplyBackdrop(this, WindowBackdropType.Mica);
        SystemThemeWatcher.Watch(this);


        //ApplicationThemeManager.Apply(ApplicationTheme.Light);

        ApplicationThemeManager.Changed += OnAppThemeChange;

        PlaceFormByTaskBarLocation();
    }

    public bool IsShown
    {
        get
        {
            return Visibility == Visibility.Visible && IsActive;
        }
    }

    private void OnAppThemeChange(ApplicationTheme currentapplicationtheme, Color systemaccent)
    {
        // TODO: IMPLEMENT FUNCTION TO CHANGE SETTINGS
        //throw new NotImplementedException();
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        // TODO: Move tome things here from the constructor
    }


    private void View_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // use the list's items source directly, this one is not filtered (otherwise searching in
        // the UI without matches could lead to the "no repositories yet"-screen)
        var hasRepositories = ListBoxRepos.ItemsSource.OfType<RepositoryViewModel>().Any();
        TbNoRepositories.SetCurrentValue(VisibilityProperty, hasRepositories ? Visibility.Collapsed : Visibility.Visible);
    }

    /// <remarks>
    ///     Window fires the Closing event before it closes. If the
    ///     user cancels the closing event, the window is not closed.
    ///     Otherwise, the window is closed and the Closed event is
    ///     fired.
    ///     Callers must have UIPermission(UIPermissionWindow.AllWindows) to call this API.
    /// </remarks>
    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    /// <summary>
    ///     Show and activates the window
    /// </summary>
    /// <remarks>
    ///     Calling Show() on window is the same as setting the
    ///     Visibility property to Visibility.Visible.
    ///     Calling Activate() calls SetForegroundWindow on the hWnd,
    ///     thus the rules for SetForegroundWindow apply to this method.
    ///     Activate() returns bool, indicating whether the window was activated or not
    /// </remarks>
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
                SearchBar_TextBox.Focus();
                SearchBar_TextBox.SelectAll();
            });
    }

    private void OnScanStateChanged(object? sender, bool isScanning)
    {
        Dispatcher.Invoke(() => ShowScanningState(isScanning));
    }

    private async void ListBoxRepos_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        // prevent double clicks from scrollbars and other non-data areas
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

    private async void ListBoxRepos_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
    {
        // This triggers only when the Context Menu is opened via right click
        // opening the context menu via Key.Left or Key.Right does not trigger this
        if (sender == null)
        {
            e.Handled = true;
            return;
        }

        var listBoxReposContextMenuOpening = await ListBoxRepos_BuildContextMenuAsync(((FrameworkElement)e.Source).ContextMenu).ConfigureAwait(true);
        if (!listBoxReposContextMenuOpening)
        {
            e.Handled = true;
        }
    }

    private async Task<bool> ListBoxRepos_BuildContextMenuAsync(ContextMenu? ctxMenu)
    {
        if (ListBoxRepos.SelectedItem is not RepositoryViewModel vm)
        {
            return false;
        }

        if (null == ctxMenu)
        {
            ListBoxRepos.SetCurrentValue(ContextMenuProperty, new ContextMenu());
            if (null != ListBoxRepos.ContextMenu)
            {
                ctxMenu = ListBoxRepos.ContextMenu;
            }
            else
            {
                return false;
            }
        }
        else
        {
            ctxMenu.Items.Clear();
        }

        try
        {
            var items = new List<Control>();

            ctxMenu.Items.Add(new MenuItem
                {
                    Header    = "Loading...",
                    IsEnabled = true,
                });

            await foreach (UserInterfaceRepositoryActionBase action in _userMenuActionFactory.CreateMenuAsync(vm.Repository).ConfigureAwait(true))
            {
                switch (action)
                {
                    case UserInterfaceSeparatorRepositoryAction:
                        {
                            if (items.Count > 0 && items[^1] is not Separator)
                            {
                                items.Add(new Separator());
                            }

                            break;
                        }

                    case DeferredSubActionsUserInterfaceRepositoryAction:
                        {
                            Control? controlItem = CreateMenuItemNewStyleAsync(action, vm);
                            if (controlItem != null)
                            {
                                items.Add(controlItem);
                            }

                            break;
                        }

                    case UserInterfaceRepositoryAction:
                        {
                            Control? controlItem = CreateMenuItemNewStyleAsync(action, vm);
                            if (controlItem != null)
                            {
                                items.Add(controlItem);
                            }

                            break;
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
        catch (Exception e)
        {
            _logger.LogError(e, "Could not create menu.");

            ctxMenu.Items.Clear();
            ctxMenu.Items.Add(new MenuItem
                {
                    Header    = "Error",
                    IsEnabled = false,
                });
            ctxMenu.Items.Add(new MenuItem
                {
                    Header    = e.Message,
                    IsEnabled = false,
                });

            return false;
        }
    }

    private async Task InvokeActionOnCurrentRepositoryAsync()
    {
        if (ListBoxRepos.SelectedItem is not RepositoryViewModel selectedView)
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
        if (RepoGrid.Visibility == Visibility.Visible)
        {
            RepoGrid.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            HelpScrollViewer.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        }
        else
        {
            RepoGrid.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            HelpScrollViewer.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
        }
    }

    private void MenuButton_Click(object sender, RoutedEventArgs e)
    {
        MenuButton.ContextMenu?.SetCurrentValue(ContextMenu.IsOpenProperty, true);
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

    private static void Navigate(string url)
    {
        Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            });
    }

    private void PlaceFormByTaskBarLocation()
    {
        SetCurrentValue(TopProperty,  SystemParameters.WorkArea.BottomRight.Y - ActualHeight - 5);
        SetCurrentValue(LeftProperty, SystemParameters.WorkArea.BottomRight.X - ActualWidth  - 10);
    }

    private void ShowUpdateIfAvailable()
    {
        var updateHint = _translationService.Translate("Update hint", App.AvailableUpdate ?? "?.?");

        UpdateButton.SetCurrentValue(VisibilityProperty, App.AvailableUpdate == null ? Visibility.Hidden : Visibility.Visible);
        UpdateButton.SetCurrentValue(ToolTipProperty,    App.AvailableUpdate == null ? "" : updateHint);
        UpdateButton.SetCurrentValue(TagProperty,        App.AvailableUpdate);

        var parent = (Grid)UpdateButton.Parent;
        parent.ColumnDefinitions[Grid.GetColumn(UpdateButton)].SetCurrentValue(ColumnDefinition.WidthProperty, App.AvailableUpdate == null ? new GridLength(0) : GridLength.Auto);
    }

    //private Control? /*MenuItem*/ CreateMenuItem(RepositoryActionBase action, RepositoryViewModel? affectedViews = null)
    //{
    //    if (action is RepositorySeparatorAction)
    //    {
    //        return new Separator();
    //    }

    //    if (action is not RepositoryAction repositoryAction)
    //    {
    //        // throw??
    //        return null;
    //    }

    //    Action<object, object> clickAction = (clickSender, clickArgs) =>
    //        {
    //            if (repositoryAction.Action is null or NullRepositoryCommand)
    //            {
    //                return;
    //            }

    //            // run actions in the UI async to not block it
    //            if (repositoryAction.ExecutionCausesSynchronizing)
    //            {
    //                Task.Run(() => SetVmSynchronizing(affectedViews, true))
    //                    .ContinueWith(t => _executor.Execute(action.Repository, action.Action))
    //                    .ContinueWith(t => SetVmSynchronizing(affectedViews, false));
    //            }
    //            else
    //            {
    //                Task.Run(() => _executor.Execute(action.Repository, action.Action));
    //            }
    //        };

    //    var item = new MenuItem
    //    {
    //        Header = repositoryAction.Name,
    //        IsEnabled = repositoryAction.CanExecute,
    //    };
    //    item.Click += new RoutedEventHandler(clickAction);

    //    // this is a deferred submenu. We want to make sure that the context menu can pop up
    //    // fast, while submenus are not evaluated yet. We don't want to make the context menu
    //    // itself slow because the creation of the submenu items takes some time.
    //    if (repositoryAction is DeferredSubActionsRepositoryAction deferredRepositoryAction && deferredRepositoryAction.DeferredSubActionsEnumerator != null)
    //    {
    //        // this is a template submenu item to enable submenus under the current
    //        // menu item. this item gets removed when the real subitems are created
    //        item.Items.Add(string.Empty);

    //        void SelfDetachingEventHandler(object _, RoutedEventArgs evtArgs)
    //        {
    //            item.SubmenuOpened -= SelfDetachingEventHandler;
    //            item.Items.Clear();

    //            foreach (RepositoryActionBase subAction in deferredRepositoryAction.DeferredSubActionsEnumerator())
    //            {
    //                Control? controlItem = CreateMenuItem(subAction);
    //                if (controlItem != null)
    //                {
    //                    item.Items.Add(controlItem);
    //                }
    //            }
    //        }

    //        item.SubmenuOpened += SelfDetachingEventHandler;
    //    }
    //    else if (repositoryAction.SubActions != null)
    //    {
    //        foreach (RepositoryActionBase subAction in repositoryAction.SubActions)
    //        {
    //            Control? controlItem = CreateMenuItem(subAction);
    //            if (controlItem != null)
    //            {
    //                item.Items.Add(controlItem);
    //            }
    //        }
    //    }

    //    return item;
    //}

    private Control? /*MenuItem*/ CreateMenuItemNewStyleAsync(UserInterfaceRepositoryActionBase action, RepositoryViewModel? affectedViews = null)
    {
        if (action is UserInterfaceSeparatorRepositoryAction)
        {
            return new Separator();
        }

        // UserInterfaceRepositoryAction
        // DeferredSubActionsUserInterfaceRepositoryAction

        if (action is not UserInterfaceRepositoryAction repositoryAction)
        {
            // throw??
            return null;
        }

        Action<object, object> clickAction = (_, _) =>
            {
                if (repositoryAction.RepositoryCommand is null or NullRepositoryCommand)
                {
                    return;
                }

                // run actions in the UI async to not block it
                if (repositoryAction.ExecutionCausesSynchronizing)
                {
                    Task.Run(() => SetVmSynchronizing(affectedViews, true))
                        .ContinueWith(_ => _executor.Execute(action.Repository, action.RepositoryCommand))
                        .ContinueWith(_ => SetVmSynchronizing(affectedViews, false));
                }
                else
                {
                    Task.Run(() => _executor.Execute(action.Repository, action.RepositoryCommand));
                }
            };

        // TODO: URGENT: The calling thread must be STA, because many UI components require this.'
        var item = new MenuItem
            {
                Header    = repositoryAction.Name,
                IsEnabled = repositoryAction.CanExecute,
            };
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
        else
        {
            if (repositoryAction.SubActions == null)
            {
                return item;
            }

            // this is a template submenu item to enable submenus under the current
            // menu item. this item gets removed when the real subitems are created
            item.Items.Add("Loading..");

            async void SelfDetachingEventHandler1(object _, RoutedEventArgs evtArgs)
            {
                item.SubmenuOpened -= SelfDetachingEventHandler1;
                item.Items.Clear();

                await Task.Run(() =>
                    {
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
                    });
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
        ScanMenuItem.SetCurrentValue(IsEnabledProperty, !isScanning);
        ScanMenuItem.SetCurrentValue(HeaderedItemsControl.HeaderProperty,
                                        isScanning
                                            ? _translationService.Translate("Scanning")
                                            : _translationService.Translate("ScanComputer"));
    }

    private bool FilterRepositories(object item)
    {
        var query = SearchBar_TextBox.Text.Trim();

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

    private void OnSearchBar_TextBoxTextChanged(object? sender, TextChangedEventArgs e)
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
                        OnSearchBar_TextBoxTextChanged(null, e);
                    });
            }

            return;
        }

        // Refresh the view
        ICollectionView view = CollectionViewSource.GetDefaultView(ListBoxRepos.ItemsSource);
        view.Refresh();
    }

    private void ListBoxRepos_ChangeCurrentItem(IndexNavigator navigator, bool focus)
    {
        if (ListBoxRepos.Items.IsEmpty) { return; }

        if (focus && !ListBoxRepos.IsFocused)
        {
            ListBoxRepos.Focus();
        }

        var currentIndex = ListBoxRepos.SelectedIndex;
        int newIndex;

        switch (navigator)
        {
            case IndexNavigator.GoToNext:
                {
                    newIndex = currentIndex + 1;
                    if (newIndex >= ListBoxRepos.Items.Count)
                    {
                        newIndex = 0;
                    }

                    break;
                }

            case IndexNavigator.GoToPrevious:
                {
                    newIndex = currentIndex - 1;
                    if (newIndex < 0)
                    {
                        newIndex = ListBoxRepos.Items.Count - 1;
                    }

                    break;
                }

            case IndexNavigator.GoToFirst:
                {
                    newIndex = 0;
                    //if (newIndex == currentIndex)
                    //{
                    //    return;
                    //}

                    break;
                }

            case IndexNavigator.GoToLast:
                {
                    newIndex = ListBoxRepos.Items.Count - 1;
                    //if (newIndex == currentIndex)
                    //{
                    //    return;
                    //}

                    break;
                }

            case IndexNavigator.StickToCurrent:
                {
                    newIndex = currentIndex;
                    break;
                }

            default:
                {
                #pragma warning disable CA2254
                    _logger.LogError(new ArgumentOutOfRangeException(nameof(navigator), navigator, null).ToString());
                #pragma warning restore CA2254
                    throw new ArgumentOutOfRangeException(nameof(navigator), navigator, null);
                }
        }

        ListBoxRepos.SetCurrentValue(Selector.SelectedIndexProperty, newIndex);
        var item = (ListBoxItem)ListBoxRepos.ItemContainerGenerator.ContainerFromIndex(newIndex);

        if (focus)
        {
            item?.Focus();
        }
    }

    private void SearchBar_TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                {
                    ListBoxRepos_ChangeCurrentItem(IndexNavigator.GoToFirst, true);
                    break;
                }

            case var _ when ListBoxRepos_NavigationKeys.TryGetValue(e.Key, out IndexNavigator kValue):
                {
                    ListBoxRepos_ChangeCurrentItem(kValue, false);
                    break;
                }
        }
    }

    private async void ListBoxRepos_KeyDown(object? sender, KeyEventArgs e)
    {
        if (null == sender || ListBoxRepos.Items.IsEmpty || ListBoxRepos.SelectedIndex < 0)
        {
            e.Handled = true;
            return;
        }

        switch (e.Key)
        {
            case Key.Enter:
                {
                    try
                    {
                        ListBoxRepos_ChangeCurrentItem(IndexNavigator.StickToCurrent, true);
                        _ = InvokeActionOnCurrentRepositoryAsync().ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Could not invoke action on current repository.");
                    }

                    break;
                }

            case var _ when ListBoxRepos_NavigationKeys.TryGetValue(e.Key, out IndexNavigator kValue):
                {
                    ListBoxRepos_ChangeCurrentItem(kValue, true);
                    break;
                }

            case Key.Left or Key.Right:
                {
                    // try open context menu.
                    ContextMenu? ctxMenu = ((FrameworkElement)e.Source).ContextMenu;

                    var listBoxReposContextMenuOpening = await ListBoxRepos_BuildContextMenuAsync(ctxMenu).ConfigureAwait(true);
                    if (listBoxReposContextMenuOpening)
                    {
                        // ReSharper disable once PossibleNullReferenceException
                        ctxMenu.Placement       = PlacementMode.Left;
                        ctxMenu.PlacementTarget = (UIElement)e.OriginalSource;
                        ctxMenu.IsOpen          = true;
                    }

                    break;
                }
        }
    }

    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        // SearchBar_TextBox.IsFocused => We deal with this in the SearchBar_TextBox_OnKeyDown method, but they are caught here too
        // ListBoxRepos.IsFocused => We deal with this in the ListBoxRepos_KeyDown method, but they are caught here too

        switch (e.Key)
        {
            case var _ when ListBoxRepos_NavigationKeys.TryGetValue(e.Key, out IndexNavigator kValue):
                {
                    ListBoxRepos_ChangeCurrentItem(kValue, true);
                    break;
                }

            case Key.F1:
                HelpButton_Click(sender, e);
                break;

            case Key.F2:
                MenuButton_Click(sender, e);
                break;

            case Key.F3:
                ScanButton_Click(sender, e);
                break;

            case Key.F4:
                ClearButton_Click(sender, e);
                break;

            case Key.F12:
                // keep window open on deactivate to make screenshots, for example
                _keepMainWindowOpenWhenLosingFocus = !_keepMainWindowOpenWhenLosingFocus;
                break;
        }
    }

    private void OnKBPress_Escape(object sender, ExecutedRoutedEventArgs e)
    {
        // triggers as soon as the Key.Escape / Key.Clear / Key.Cancel is pressed, not when it is released. The equivalent of OnPreviewKeyDown.

        if (string.IsNullOrEmpty(SearchBar_TextBox.Text))
        {
            Hide();
        }
        else
        {
            SearchBar_TextBox.Clear();
            ListBoxRepos.UnselectAll();
            SearchBar_TextBox.Focus();
        }
    }

    /// <summary>
    ///     This event is raised when the window is activated
    /// </summary>
    private void MainWindow_OnActivated(object? sender, EventArgs e)
    {
        ShowUpdateIfAvailable();
        SearchBar_TextBox.Focus();
        SearchBar_TextBox.SelectAll();
    }

    /// <summary>
    ///     This event is raised when the window becomes a background window.
    /// </summary>
    /// <remarks>
    ///     A window is deactivated (becomes a background window) when:
    ///     * A user switches to another window in the current application.
    ///     * A user switches to the window in another application by using ALT+TAB or by using Task Manager.
    ///     * A user clicks the taskbar button for a window in another application.
    /// </remarks>
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.windows.window.deactivated" />
    private void MainWindow_OnDeactivated(object? sender, EventArgs e)
    {
        if (_keepMainWindowOpenWhenLosingFocus)
        {
            return;
        }

        /*
            * Calling Hide on window is the same as setting the
            * Visibility property to Visibility.Hidden
            */
        Hide();
    }

    /// <summary>
    ///     This event is raised when the window and its content is rendered.
    /// </summary>
    private void MainWindow_OnContentRendered(object? sender, EventArgs e)
    {
        // TODO
    }

    /// <summary>
    ///     This even fires after the window source is created before it is shown.
    /// </summary>
    /// <remarks> It enables connection to the Win32 API. </remarks>
    private void MainWindow_OnSourceInitialized(object? sender, EventArgs e)
    {
        // TODO
    }

}

