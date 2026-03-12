namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using DynamicData;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.App.Controls;
using RepoM.App.Plugins;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryOrdering;
using RepoM.App.Services;
using RepoM.App.ViewModels;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Pinning;
using RepoM.Core.Repositories.Store;
using SourceChord.FluentWPF;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfContextMenu = System.Windows.Controls.ContextMenu;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private bool _closeOnDeactivate = true;
    private static readonly bool _useOffScreenHide =
        string.Equals(Environment.GetEnvironmentVariable("REPOM_HIDE_OFFSCREEN"), "1", StringComparison.Ordinal);
    private readonly IRepositoryIgnoreStore _repositoryIgnoreStore;
    private readonly RepositoryMonitorService _monitorService;
    private readonly IRepositoryStore _store;
    private readonly IPinningService _pinningService;
    private readonly ITranslationService _translationService;
    private readonly IFileSystem _fileSystem;
    private readonly ActionExecutor _executor;
    private readonly IRepositoryFilteringManager _repositoryFilteringManager;
    private readonly ILogger _logger;
    private readonly IUserMenuActionMenuFactory _userMenuActionFactory;
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IRepositoryComparerManager _repositoryComparerManager;
    private readonly IThreadDispatcher _threadDispatcher;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IModuleManager _moduleManager;
    private ReadOnlyObservableCollection<RepositoryViewModel> _repositories = null!;
    private readonly CompositeDisposable _disposables = new();
    private bool _isScanning;
    private readonly object _separator = new();
    private readonly object _singleItem = new();
    private readonly object _menuItem = new();

    public MainWindow(
        RepositoryMonitorService monitorService,
        IRepositoryStore store,
        IPinningService pinningService,
        IRepositoryIgnoreStore repositoryIgnoreStore,
        IAppSettingsService appSettingsService,
        ITranslationService translationService,
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        ActionExecutor executor,
        IRepositoryComparerManager repositoryComparerManager,
        IThreadDispatcher threadDispatcher,
        IRepositoryFilteringManager repositoryFilteringManager,
        IModuleManager moduleManager,
        ILogger logger,
        IUserMenuActionMenuFactory userMenuActionFactory)
    {
        _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _pinningService = pinningService ?? throw new ArgumentNullException(nameof(pinningService));
        _repositoryFilteringManager = repositoryFilteringManager ?? throw new ArgumentNullException(nameof(repositoryFilteringManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userMenuActionFactory = userMenuActionFactory ?? throw new ArgumentNullException(nameof(userMenuActionFactory));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryIgnoreStore = repositoryIgnoreStore ?? throw new ArgumentNullException(nameof(repositoryIgnoreStore));
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _repositoryComparerManager = repositoryComparerManager ?? throw new ArgumentNullException(nameof(repositoryComparerManager));
        _threadDispatcher = threadDispatcher ?? throw new ArgumentNullException(nameof(threadDispatcher));
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _moduleManager = moduleManager ?? throw new ArgumentNullException(nameof(moduleManager));

        InitializeComponent();

        SetAcrylicWindowStyle(this, AcrylicWindowStyle.None);

        Loaded += (_, _) =>
        {
            if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            {
                hwndSource.AddHook(ResizeHook);
            }
        };
    }

    private const int WM_ENTERSIZEMOVE = 0x0231;
    private const int WM_EXITSIZEMOVE = 0x0232;

    private IntPtr ResizeHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_ENTERSIZEMOVE:
                AcrylicWindow.SetEnabled(this, false);
                break;
            case WM_EXITSIZEMOVE:
                AcrylicWindow.SetEnabled(this, true);
                break;
        }

        return IntPtr.Zero;
    }

    private void UpdateNoRepositoriesVisibility()
    {
        // Show "no repositories" only when the store is truly empty (not just filtered to zero results).
        var hasRepositories = _store.Count > 0;
        Dispatcher.InvokeAsync(() =>
            tbNoRepositories.Visibility = hasRepositories ? Visibility.Hidden : Visibility.Visible);
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        ShowUpdateIfAvailable();
        if (!_monitorService.IsStalenessCheckRunning)
        {
            Task.Run(() => _monitorService.RemoveStaleRepositories());
        }

        Task.Run(() => _monitorService.RefreshAllAsync());

        txtFilter.Focus();
        txtFilter.SelectAll();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);

        if (_closeOnDeactivate)
        {
            HideWindow();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        HideWindow();
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
            HideWindow();
        }
    }

    /// <summary>
    /// Hides the window. When REPOM_HIDE_OFFSCREEN=1, moves the window off-screen instead of
    /// calling <see cref="Window.Hide"/> so the DWM acrylic composition stays alive and avoids
    /// a white flash on re-show.
    /// </summary>
    private void HideWindow()
    {
        if (_useOffScreenHide)
        {
            Left = -99999;
            Top = -99999;
        }
        else
        {
            Hide();
        }
    }

    public void SetReady()
    {
        Dispatcher.Invoke(() =>
        {
            var orderingsViewModel = new OrderingsViewModel(_repositoryComparerManager, _threadDispatcher);
            var queryParsersViewModel = new QueryParsersViewModel(_repositoryFilteringManager, _threadDispatcher);
            var filterViewModel = new FiltersViewModel(_repositoryFilteringManager, _threadDispatcher);
            var pluginsViewModel = new PluginCollectionViewModel(_moduleManager);

            DataContext = new MainWindowViewModel(
                _appSettingsService,
                orderingsViewModel,
                queryParsersViewModel,
                filterViewModel,
                pluginsViewModel,
                new HelpViewModel(_translationService));
            SettingsMenu.DataContext = DataContext; // this is out of the visual tree

            var uiScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current!);

            // Subscribe to scan state
            var scanSubscription = _monitorService.IsScanning
                .ObserveOn(uiScheduler)
                .Subscribe(isScanning => ShowScanningState(isScanning));
            _disposables.Add(scanSubscription);

            // --- DynamicData pipeline: filter & sort on background, bind on UI ---
            _filterTextSubject = new BehaviorSubject<string>(string.Empty);
            _disposables.Add(_filterTextSubject);

            var filterObservable = _repositoryFilteringManager.CreateFilterObservable(
                _filterTextSubject.Throttle(TimeSpan.FromMilliseconds(200)).DistinctUntilChanged());

            var bindSubscription = _store.Connect()
                .TransformWithInlineUpdate(
                    info => new RepositoryViewModel(info, _pinningService),
                    (existingVm, updatedInfo) => existingVm.Update(updatedInfo))
                .Filter(filterObservable)
                .Batch(TimeSpan.FromMilliseconds(200))
                .ObserveOn(uiScheduler)
                .SortAndBind(out _repositories, _repositoryComparerManager.SortObservable)
                .Subscribe();
            _disposables.Add(bindSubscription);

            lstRepositories.ItemsSource = _repositories;

            // Track whether we have any repositories at all (unfiltered count)
            var countSubscription = _store.Connect()
                .Subscribe(_ => UpdateNoRepositoriesVisibility());
            _disposables.Add(countSubscription);

            PlaceFormByTaskBarLocation();

            loadingOverlay.Visibility = Visibility.Collapsed;
        });
    }

    private BehaviorSubject<string>? _filterTextSubject;

    public void ShowAndActivate()
    {
        Dispatcher.Invoke(() =>
            {
                PlaceFormByTaskBarLocation();
                Show();
                Activate();
                txtFilter.Focus();
                txtFilter.SelectAll();
            });
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

        WpfContextMenu ctxMenu = ((FrameworkElement)e.Source).ContextMenu!;
        var lstRepositoriesContextMenuOpening = await LstRepositoriesContextMenuOpeningWrapperAsync(ctxMenu).ConfigureAwait(true);
        if (!lstRepositoriesContextMenuOpening)
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

    private async Task<bool> LstRepositoriesContextMenuOpeningAsync(WpfContextMenu ctxMenu)
    {
        if (lstRepositories.SelectedItem is not RepositoryViewModel vm)
        {
            return false;
        }

        int AddItemMenuAndSeparator(int count)
        {
            ctxMenu.Items.Add(new AcrylicMenuItem
            {
                Header = string.Empty,
                Visibility = Visibility.Collapsed,
                Tag = _singleItem,
                IsEnabled = default,
            });
            ctxMenu.Items.Add(new AcrylicMenuItem
            {
                Header = string.Empty,
                Visibility = Visibility.Collapsed,
                Items = { new Separator(), },
                IsEnabled = default,
                Tag = _menuItem,
            });
            ctxMenu.Items.Add(new Separator
            {
                Visibility = Visibility.Collapsed,
                Tag = _separator,
            });

            return count + 3;
        }

        // Phase 1: Collect all actions off the UI thread to avoid per-item
        // dispatcher marshaling and intermediate layout passes.
        var actions = new List<UserInterfaceRepositoryActionBase>();
        await foreach (UserInterfaceRepositoryActionBase action in _userMenuActionFactory.CreateMenuAsync(vm.Repository).ConfigureAwait(false))
        {
            actions.Add(action);
        }

        // Phase 2: Marshal back to UI thread once and apply all changes
        // in a single synchronous batch — no intermediate layout passes.
        await Dispatcher.InvokeAsync(() => ApplyMenuActions(ctxMenu, actions, vm, AddItemMenuAndSeparator));

        return true;
    }

    private void ApplyMenuActions(
        WpfContextMenu ctxMenu,
        List<UserInterfaceRepositoryActionBase> actions,
        RepositoryViewModel vm,
        Func<int, int> addItemMenuAndSeparator)
    {
        var index = -1;
        var lastVisibleSeparator = false;
        var ctxMenuItemsCount = ctxMenu.Items.Count;

        foreach (UserInterfaceRepositoryActionBase action in actions)
        {
            index++;

            if (action is UserInterfaceSeparatorRepositoryAction)
            {
                lastVisibleSeparator = ApplySeparator(ctxMenu, ref index, ref ctxMenuItemsCount, lastVisibleSeparator, addItemMenuAndSeparator);
            }
            else if (action is UserInterfaceRepositoryAction uira)
            {
                lastVisibleSeparator = false;
                ApplyMenuItem(ctxMenu, ref index, ref ctxMenuItemsCount, uira, vm, addItemMenuAndSeparator);
            }
        }

        if (!lastVisibleSeparator)
        {
            index++;
        }

        CollapseItemsFrom(ctxMenu, index, ctxMenuItemsCount);
    }

    private static bool ApplySeparator(
        WpfContextMenu ctxMenu,
        ref int index,
        ref int ctxMenuItemsCount,
        bool lastVisibleSeparator,
        Func<int, int> addItemMenuAndSeparator)
    {
        SkipAndCollapse(ctxMenu, ref index, ctxMenuItemsCount, item => item is AcrylicMenuItem);

        if (ctxMenuItemsCount <= index)
        {
            ctxMenuItemsCount = addItemMenuAndSeparator(ctxMenuItemsCount);
            index += 2;
        }

        if (ctxMenu.Items[index] is Separator s)
        {
            s.Visibility = lastVisibleSeparator ? Visibility.Collapsed : Visibility.Visible;
        }

        return true;
    }

    private void ApplyMenuItem(
        WpfContextMenu ctxMenu,
        ref int index,
        ref int ctxMenuItemsCount,
        UserInterfaceRepositoryAction uira,
        RepositoryViewModel vm,
        Func<int, int> addItemMenuAndSeparator)
    {
        var hasSubItems = HasSubItems(uira);
        var skipTag = hasSubItems ? _singleItem : _menuItem;

        SkipAndCollapse(ctxMenu, ref index, ctxMenuItemsCount, item => item is Separator || (item is AcrylicMenuItem ami && ami.Tag == skipTag));

        if (ctxMenuItemsCount <= index)
        {
            ctxMenuItemsCount = addItemMenuAndSeparator(ctxMenuItemsCount);
            index += hasSubItems ? 1 : 0;
        }

        var acrylicMenuItem = (AcrylicMenuItem)ctxMenu.Items[index]!;
        if (acrylicMenuItem.Visibility != Visibility.Visible)
        {
            acrylicMenuItem.Visibility = Visibility.Visible;
        }

        acrylicMenuItem.SetHeader(uira.Name);
        acrylicMenuItem.SetEnabled(uira.CanExecute);

        if (hasSubItems)
        {
            SetSubMenu(acrylicMenuItem, uira);
        }
        else
        {
            SetClick(acrylicMenuItem, uira, vm);
        }
    }

    private static void SkipAndCollapse(WpfContextMenu ctxMenu, ref int index, int itemsCount, Func<object, bool> shouldSkip)
    {
        while (itemsCount > index && shouldSkip(ctxMenu.Items[index]!))
        {
            var ctrl = (Control)ctxMenu.Items[index]!;
            if (ctrl.Visibility != Visibility.Collapsed)
            {
                ctrl.Visibility = Visibility.Collapsed;
            }

            index++;
        }
    }

    private static void CollapseItemsFrom(WpfContextMenu ctxMenu, int startIndex, int itemsCount)
    {
        for (var i = startIndex; i < itemsCount; i++)
        {
            var ctrl = (Control)ctxMenu.Items[i]!;
            if (ctrl.Visibility != Visibility.Collapsed)
            {
                ctrl.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void SetClick(AcrylicMenuItem acrylicMenuItem, UserInterfaceRepositoryAction action, RepositoryViewModel? affectedViews)
    {
        void ClickAction(object clickSender, object clickArgs)
        {
            // run actions in the UI async to not block it
            if (action.ExecutionCausesSynchronizing)
            {
                Task.Run(() => SetVmSynchronizing(affectedViews, true))
                    .ContinueWith(t => _executor.Execute(action.Repository, action.RepositoryCommand))
                    .ContinueWith(t => SetVmSynchronizing(affectedViews, false));
            }
            else
            {
                Task.Run(() => _executor.Execute(action.Repository, action.RepositoryCommand));
            }
        }

        if (action.RepositoryCommand is null or NullRepositoryCommand)
        {
            acrylicMenuItem.ClearClick();
        }
        else
        {
            acrylicMenuItem.SetClick(new RoutedEventHandler((Action<object, object>)ClickAction));
        }
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
            WpfContextMenu? ctxMenu = ((FrameworkElement)e.Source).ContextMenu;
            if (ctxMenu == null)
            {
                e.Handled = true;
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
        if (_isScanning)
        {
            _monitorService.CancelAllScans();
            ScanMenuItem.Header = _translationService.Translate("Stopping");
            ScanMenuItem.IsEnabled = false;
        }
        else
        {
            _ = _monitorService.ScanAsync();
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _monitorService.CancelAllScans();
        _store.Clear();
        _ = _monitorService.ScanAsync();
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

    private Control? CreateMenuItemAsync(UserInterfaceRepositoryActionBase action, RepositoryViewModel? affectedViews = null)
    {
        if (action is UserInterfaceSeparatorRepositoryAction)
        {
            return new Separator();
        }

        if (action is not UserInterfaceRepositoryAction repositoryAction)
        {
            // throw??
            return null;
        }

        var item = new AcrylicMenuItem
        {
            Header = repositoryAction.Name,
            IsEnabled = repositoryAction.CanExecute,
        };
        SetClick(item, repositoryAction, affectedViews);
        SetSubMenu(item, repositoryAction);
        return item;
    }

    private static bool HasSubItems(UserInterfaceRepositoryAction repositoryAction)
    {
        if (repositoryAction is DeferredSubActionsUserInterfaceRepositoryAction)
        {
            return true;
        }

        return repositoryAction.SubActions != null;
    }

    private void SetSubMenu(AcrylicMenuItem item, UserInterfaceRepositoryAction repositoryAction)
    {
        if (repositoryAction is DeferredSubActionsUserInterfaceRepositoryAction deferredRepositoryAction)
        {
            EnsureTemplateSeparator(item);
            item.LoadData(deferredRepositoryAction);
            item.SetSubMenuOpened(async (_, _) =>
            {
                item.ClearSubMenuOpened();
                item.ClearItems();
                PopulateSubMenuItems(item, await item.DataTask);
                item.ClearData();
            });
        }
        else if (repositoryAction.SubActions != null)
        {
            EnsureTemplateSeparator(item);
            item.SetSubMenuOpened((_, _) =>
            {
                item.ClearSubMenuOpened();
                item.ClearItems();
                PopulateSubMenuItems(item, repositoryAction.SubActions);
            });
        }
    }

    private static void EnsureTemplateSeparator(AcrylicMenuItem item)
    {
        if (item.Items.Count == 0)
        {
            item.Items.Add(new Separator());
        }
    }

    private void PopulateSubMenuItems(AcrylicMenuItem item, IEnumerable<UserInterfaceRepositoryActionBase> subActions)
    {
        foreach (UserInterfaceRepositoryActionBase subAction in subActions)
        {
            Control? controlItem = CreateMenuItemAsync(subAction);
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

        if (item.Items.Count > 0 && item.Items[^1] is Separator)
        {
            item.Items.RemoveAt(item.Items.Count - 1);
        }
    }

    private static void SetVmSynchronizing(RepositoryViewModel? affectedVm, bool synchronizing)
    {
        affectedVm?.IsSynchronizing = synchronizing;
    }

    private void ShowScanningState(bool isScanning)
    {
        _logger.LogInformation("UI scan state changed: IsScanning = {IsScanning}", isScanning);
        _isScanning = isScanning;
        ScanMenuItem.IsEnabled = true;
        ScanMenuItem.Header = isScanning
            ? _translationService.Translate("StopScanning")
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
        _filterTextSubject?.OnNext(txtFilter.Text.Trim());
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

    public bool IsShown => Visibility == Visibility.Visible && IsActive && (!_useOffScreenHide || Left > -99000);
}
