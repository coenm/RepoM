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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using DynamicData;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.App.Plugins;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryOrdering;
using RepoM.App.Services;
using RepoM.App.ViewModels;
using RepoM.Api.QuickFilter;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Favorite;
using RepoM.Core.Repositories.Store;
using SourceChord.FluentWPF;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private bool _closeOnDeactivate = true;
    private static readonly bool _useOffScreenHide =
        string.Equals(Environment.GetEnvironmentVariable("REPOM_HIDE_OFFSCREEN"), "1", StringComparison.Ordinal);

    private enum AcrylicBehavior
    {
        /// <summary>
        /// Current/legacy behavior: acrylic enabled, but temporarily disabled during resize.
        /// </summary>
        Legacy = 0,

        /// <summary>
        /// No acrylic effects at all.
        /// </summary>
        Disabled = 1,

        /// <summary>
        /// Acrylic always enabled and never disabled during resize.
        /// </summary>
        AlwaysOn = 2,
    }

    private static readonly AcrylicBehavior _acrylicBehavior = GetAcrylicBehavior();

    private readonly IRepositoryIgnoreStore _repositoryIgnoreStore;
    private readonly RepositoryMonitorService _monitorService;
    private readonly IRepositoryStore _store;
    private readonly IFavoriteService _favoriteService;
    private readonly IRepositoryMonitoringService _monitoringService;
    private readonly IRepositoryMonitoringEvents _monitoringEvents;
    private readonly ITranslationService _translationService;
    private readonly IFileSystem _fileSystem;
    private readonly IRepositoryFilteringManager _repositoryFilteringManager;
    private readonly ILogger _logger;
    private readonly IAppDataPathProvider _appDataPathProvider;
    private readonly IRepositoryComparerManager _repositoryComparerManager;
    private readonly IThreadDispatcher _threadDispatcher;
    private readonly IAppSettingsService _appSettingsService;
    private readonly IModuleManager _moduleManager;
    private readonly QuickFilterBarViewModel _quickFilterBarViewModel;
    private readonly RepositoryListViewModel _repositoryListViewModel;
    private readonly IUserMenuActionMenuFactory _userMenuActionFactory;
    private ReadOnlyObservableCollection<RepositoryViewModel> _repositories = null!;
    private readonly CompositeDisposable _disposables = new();
    private bool _isScanning;

    public MainWindow(
        RepositoryMonitorService monitorService,
        IRepositoryStore store,
        IFavoriteService favoriteService,
        IRepositoryMonitoringService monitoringService,
        IRepositoryMonitoringEvents monitoringEvents,
        IRepositoryIgnoreStore repositoryIgnoreStore,
        IAppSettingsService appSettingsService,
        ITranslationService translationService,
        IAppDataPathProvider appDataPathProvider,
        IFileSystem fileSystem,
        ActionExecutor executor,
        IRepositoryComparerManager repositoryComparerManager,
        IThreadDispatcher threadDispatcher,
        IRepositoryFilteringManager repositoryFilteringManager,
        IQuickFilterService quickFilterService,
        IEnumerable<INamedQueryParser> namedQueryParsers,
        IModuleManager moduleManager,
        ILogger logger,
        IUserMenuActionMenuFactory userMenuActionFactory)
    {
        _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _favoriteService = favoriteService ?? throw new ArgumentNullException(nameof(favoriteService));
        _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
        _monitoringEvents = monitoringEvents ?? throw new ArgumentNullException(nameof(monitoringEvents));
        _repositoryFilteringManager = repositoryFilteringManager ?? throw new ArgumentNullException(nameof(repositoryFilteringManager));
        ArgumentNullException.ThrowIfNull(quickFilterService);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(userMenuActionFactory);
        _userMenuActionFactory = userMenuActionFactory;
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryIgnoreStore = repositoryIgnoreStore ?? throw new ArgumentNullException(nameof(repositoryIgnoreStore));
        _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        ArgumentNullException.ThrowIfNull(executor);
        _repositoryComparerManager = repositoryComparerManager ?? throw new ArgumentNullException(nameof(repositoryComparerManager));
        _threadDispatcher = threadDispatcher ?? throw new ArgumentNullException(nameof(threadDispatcher));
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _moduleManager = moduleManager ?? throw new ArgumentNullException(nameof(moduleManager));
        _quickFilterBarViewModel = new QuickFilterBarViewModel(quickFilterService, repositoryFilteringManager, namedQueryParsers, logger);
        _repositoryListViewModel = new RepositoryListViewModel(_monitorService, executor, userMenuActionFactory, _logger, _quickFilterBarViewModel.AddTagCommand)
            {
                MenuPrefetchHoverDelay = TimeSpan.FromMilliseconds(_appSettingsService.MenuPrefetchHoverDelayMilliseconds),
            };

        InitializeComponent();
        quickFilterBar.Initialize(_quickFilterBarViewModel);
        repositoryList.Initialize(_repositoryListViewModel);

        SetAcrylicWindowStyle(this, AcrylicWindowStyle.None);

        // Configure acrylic behavior based on environment variable.
        switch (_acrylicBehavior)
        {
            case AcrylicBehavior.Disabled:
                AcrylicWindow.SetEnabled(this, false);
                break;
            case AcrylicBehavior.AlwaysOn:
                AcrylicWindow.SetEnabled(this, true);
                break;
            case AcrylicBehavior.Legacy:
            default:
                // Keep existing behavior.
                break;
        }

        Loaded += (_, _) =>
        {
            // In legacy mode we keep the old behavior: disable acrylic during resize.
            if (_acrylicBehavior != AcrylicBehavior.Legacy)
            {
                return;
            }

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
        // Only apply the resize behavior in legacy mode; in other modes we leave
        // acrylic either always off or always on.
        if (_acrylicBehavior != AcrylicBehavior.Legacy)
        {
            return IntPtr.Zero;
        }

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

    // Generates the context menu once for a real repository so the first user-triggered open
    // does not pay the one-time JIT/initialization cost of the Scriban evaluation pipeline.
    private void WarmupContextMenu()
    {
        RepositoryInfo? info = _store.Items.FirstOrDefault();
        if (info == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var repository = new RepositoryInfoAdapter(info);
                await foreach (var _ in _userMenuActionFactory.CreateMenuAsync(repository).ConfigureAwait(false))
                {
                    // Enumerate fully to warm the evaluation path; results are intentionally discarded.
                }

                _logger.LogDebug("Context menu warmup completed");
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Context menu warmup failed");
            }
        });
    }

    protected override async void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        ShowUpdateIfAvailable();

        txtFilter.Focus();
        txtFilter.SelectAll();

        if (!_monitorService.IsStalenessCheckRunning)
        {
            Task.Run(() => _monitorService.RemoveStaleRepositories());
        }

        // Await so the UI (incl. context menu actions) is based on current git refs.
        await _monitorService.RefreshAllAsync();
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
            var quickFilterCommands = new MainWindowQuickFilterCommands(
                _quickFilterBarViewModel.SaveSearchTextCommand,
                _quickFilterBarViewModel.AddTagCommand);

            DataContext = new MainWindowViewModel(
                _appSettingsService,
                orderingsViewModel,
                queryParsersViewModel,
                filterViewModel,
                pluginsViewModel,
                new HelpViewModel(_translationService),
                quickFilterCommands);
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

            var quickFilterChanged = Observable.FromEventPattern(
                    h => _quickFilterBarViewModel.FilterStateChanged += h,
                    h => _quickFilterBarViewModel.FilterStateChanged -= h)
                .Select(_ => System.Reactive.Unit.Default)
                .StartWith(System.Reactive.Unit.Default);

            var baseFilterObservable = _repositoryFilteringManager.CreateFilterObservable(
                _filterTextSubject.Throttle(TimeSpan.FromMilliseconds(200)).DistinctUntilChanged());

            // Combine the existing filter with quick filters
            var filterObservable = baseFilterObservable
                .CombineLatest(quickFilterChanged, (basePredicate, _) =>
                {
                    var quickQuery = _quickFilterBarViewModel.GetCombinedActiveQuery();
                    if (quickQuery == null)
                    {
                        return basePredicate;
                    }

                    var matcher = Bootstrapper.Container.GetInstance<IRepositoryMatcher>();
                    return (RepositoryViewModel vm) =>
                        basePredicate(vm) && matcher.Matches(vm.Repository, quickQuery);
                });

            var bindSubscription = _store.Connect()
                .TransformWithInlineUpdate(
                    info => new RepositoryViewModel(info, _favoriteService, _monitoringService, _monitoringEvents),
                    (existingVm, updatedInfo) => existingVm.Update(updatedInfo))
                .DisposeMany()
                .Filter(filterObservable)
                .Batch(TimeSpan.FromMilliseconds(200))
                .ObserveOn(uiScheduler)
                .SortAndBind(out _repositories, _repositoryComparerManager.SortObservable)
                .Subscribe();
            _disposables.Add(bindSubscription);

            _repositoryListViewModel.ItemsSource = _repositories;

            // Track whether we have any repositories at all (unfiltered count)
            var countSubscription = _store.Connect()
                .Subscribe(_ => UpdateNoRepositoriesVisibility());
            _disposables.Add(countSubscription);

            // Warm up the context-menu evaluation path once, in the background, using the first
            // discovered repository so the first real menu open is fast (JITs the Scriban evaluation).
            var menuWarmupSubscription = _store.Connect()
                .Where(_ => _store.Count > 0)
                .Take(1)
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ => WarmupContextMenu());
            _disposables.Add(menuWarmupSubscription);

            PlaceFormByTaskBarLocation();

            loadingOverlay.Visibility = Visibility.Collapsed;
            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)));
        });
    }

    private BehaviorSubject<string>? _filterTextSubject;

    public void ShowAndActivate()
    {
        Dispatcher.Invoke(() =>
            {
                Opacity = 0;
                PlaceFormByTaskBarLocation();
                Show();
                Activate();
                txtFilter.Focus();
                txtFilter.SelectAll();
                BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)));
            });
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

        if (!_fileSystem.Directory.Exists(directoryName))
        {
            return;
        }

        Process.Start(new ProcessStartInfo(directoryName)
            {
                UseShellExecute = true,
            });
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
            repositoryList.FocusList();
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
        repositoryList.FocusFirstItem();
    }

    public bool IsShown => Visibility == Visibility.Visible && IsActive && (!_useOffScreenHide || Left > -99000);

    private static AcrylicBehavior GetAcrylicBehavior()
    {
        string? value = Environment.GetEnvironmentVariable("REPOM_ACRYLIC_MODE");

        return value switch
        {
            "0" => AcrylicBehavior.Disabled,
            "1" => AcrylicBehavior.AlwaysOn,
            _ => AcrylicBehavior.Legacy,
        };
    }
}
