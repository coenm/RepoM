namespace RepoM.UI.ViewModels;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.Git;
using RepoM.UI.RepositoryActions;
using RepoM.UI.Services;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Model;

public sealed class RepositoryListViewModel : INotifyPropertyChanged
{
    private readonly IRepositoryMonitorService _monitorService;
    private readonly ActionExecutor _executor;
    private readonly IUserMenuActionMenuFactory _userMenuActionMenuFactory;
    private readonly ILogger _logger;
    private IEnumerable? _itemsSource;
    private RepositoryViewModel? _selectedRepository;

    private readonly object _prefetchLock = new();
    private RepositoryViewModel? _prefetchRepository;
    private CancellationTokenSource? _prefetchCts;
    private Task<IReadOnlyList<IRepositoryMenuEntryViewModel>>? _prefetchTask;

    public RepositoryListViewModel(
        IRepositoryMonitorService monitorService,
        ActionExecutor executor,
        IUserMenuActionMenuFactory userMenuActionMenuFactory,
        ILogger logger,
        ICommand addQuickFilterTagCommand)
    {
        _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _userMenuActionMenuFactory = userMenuActionMenuFactory ?? throw new ArgumentNullException(nameof(userMenuActionMenuFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        AddQuickFilterTagCommand = addQuickFilterTagCommand ?? throw new ArgumentNullException(nameof(addQuickFilterTagCommand));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        set
        {
            if (ReferenceEquals(_itemsSource, value))
            {
                return;
            }

            _itemsSource = value;
            OnPropertyChanged();
        }
    }

    public RepositoryViewModel? SelectedRepository
    {
        get => _selectedRepository;
        set
        {
            if (ReferenceEquals(_selectedRepository, value))
            {
                return;
            }

            _selectedRepository = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddQuickFilterTagCommand { get; }

    // Configurable via app settings; how long the mouse must hover before the menu is prefetched.
    public TimeSpan MenuPrefetchHoverDelay { get; set; } = TimeSpan.Zero;

    internal Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> CreateContextMenuEntriesAsync(CancellationToken cancellationToken)
    {
        if (SelectedRepository is not RepositoryViewModel selectedRepository)
        {
            return Task.FromResult<IReadOnlyList<IRepositoryMenuEntryViewModel>>([]);
        }

        // Reuse the background prefetch (started on hover) when it targets the repository being opened.
        lock (_prefetchLock)
        {
            if (_prefetchTask is not null && _prefetchCts is not null && ReferenceEquals(_prefetchRepository, selectedRepository))
            {
                Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> prefetched = _prefetchTask;
                CancellationTokenSource prefetchCts = _prefetchCts;
                _logger.LogInformation("Context menu prefetch hit for {RepositoryPath}; ready={Ready}", selectedRepository.Path, prefetched.IsCompletedSuccessfully);
                ClearPrefetch();
                return AwaitPrefetchAsync(prefetched, prefetchCts, selectedRepository, cancellationToken);
            }
        }

        _logger.LogInformation("Context menu prefetch miss for {RepositoryPath}", selectedRepository.Path);
        return Task.Run(() => BuildContextMenuEntriesAsync(selectedRepository, cancellationToken), cancellationToken);
    }

    // Takes ownership of the prefetch CTS so the caller's cancellation also stops the prefetch.
    private async Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> AwaitPrefetchAsync(
        Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> prefetched,
        CancellationTokenSource prefetchCts,
        RepositoryViewModel repository,
        CancellationToken cancellationToken)
    {
        using (prefetchCts)
        using (cancellationToken.Register(static state => ((CancellationTokenSource)state!).Cancel(), prefetchCts))
        {
            try
            {
                return await prefetched.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogDebug(exception, "Context menu prefetch failed; building the menu again.");
            }
        }

        return await Task.Run(() => BuildContextMenuEntriesAsync(repository, cancellationToken), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Starts building the context menu for <paramref name="repository"/> in the background (triggered
    /// after the user hovers a repository). Any prefetch for a different repository is cancelled first,
    /// so switching from one repository to another stops the previous background build.
    /// </summary>
    internal void StartContextMenuPrefetch(RepositoryViewModel repository)
    {
        ArgumentNullException.ThrowIfNull(repository);

        lock (_prefetchLock)
        {
            if (_prefetchTask is not null && ReferenceEquals(_prefetchRepository, repository))
            {
                return;
            }

            CancelPrefetch();

            var cts = new CancellationTokenSource();
            _prefetchCts = cts;
            _prefetchRepository = repository;
            CancellationToken token = cts.Token;
            _logger.LogDebug("Context menu hover prefetch started for {RepositoryPath}", repository.Path);
            _prefetchTask = Task.Run(() => BuildContextMenuEntriesAsync(repository, token), token);
            _ = ObservePrefetchAsync(_prefetchTask, repository.Path);
        }
    }

    private async Task ObservePrefetchAsync(Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> task, string repositoryPath)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (task.IsCanceled)
        {
            _logger.LogDebug("Context menu hover prefetch cancelled for {RepositoryPath}", repositoryPath);
        }
        catch (Exception exception)
        {
            // Observe abandoned hover tasks; the original task still faults so opening can retry.
            _logger.LogWarning(exception, "Context menu hover prefetch failed for {RepositoryPath}", repositoryPath);
        }
    }

    private async Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> BuildContextMenuEntriesAsync(RepositoryViewModel selectedRepository, CancellationToken cancellationToken)
    {
        var buildId = Guid.NewGuid();
        var timer = Stopwatch.StartNew();
        var stage = "monitoring";
        var outcome = "failed";
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            selectedRepository.EnableMonitoring();

            stage = "git-refresh";
            var refreshTimer = Stopwatch.StartNew();
            RepositoryInfo? updatedInfo = await _monitorService.RefreshRepositoryAsync(selectedRepository.Path, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Context menu {MenuBuildId} Git refresh for {RepositoryPath}: {ElapsedMs:F1} ms; snapshotAvailable={SnapshotAvailable}",
                buildId, selectedRepository.Path, refreshTimer.Elapsed.TotalMilliseconds, updatedInfo != null);
            cancellationToken.ThrowIfCancellationRequested();
            IRepository repositoryForMenu = updatedInfo != null ? new RepositoryInfoAdapter(updatedInfo) : selectedRepository.Repository;

            stage = "menu-evaluation";
            var evaluationTimer = Stopwatch.StartNew();
            var itemTimer = Stopwatch.StartNew();
            var entries = new List<IRepositoryMenuEntryViewModel>();
            await foreach (UserInterfaceRepositoryActionBase action in _userMenuActionMenuFactory.CreateMenuAsync(repositoryForMenu).WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                _logger.LogDebug("Context menu {MenuBuildId} yielded {MenuItem} after {ElapsedMs:F1} ms",
                    buildId, action is UserInterfaceRepositoryAction named ? named.Name : action.GetType().Name, itemTimer.Elapsed.TotalMilliseconds);
                IRepositoryMenuEntryViewModel? entry = CreateMenuEntry(action, selectedRepository, buildId);
                if (entry != null)
                {
                    entries.Add(entry);
                }

                itemTimer.Restart();
            }

            _logger.LogInformation("Context menu {MenuBuildId} evaluation for {RepositoryPath}: {ElapsedMs:F1} ms; entries={EntryCount}",
                buildId, selectedRepository.Path, evaluationTimer.Elapsed.TotalMilliseconds, entries.Count);
            outcome = "completed";
            return entries;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            outcome = "cancelled";
            throw;
        }
        finally
        {
            _logger.LogInformation("Context menu {MenuBuildId} build for {RepositoryPath}: {ElapsedMs:F1} ms; outcome={Outcome}; stage={Stage}",
                buildId, selectedRepository.Path, timer.Elapsed.TotalMilliseconds, outcome, stage);
        }
    }

    // Cancels and forgets any in-flight prefetch.
    private void CancelPrefetch()
    {
        if (_prefetchCts is not null)
        {
            _prefetchCts.Cancel();
            _prefetchCts.Dispose();
            _prefetchCts = null;
        }

        _prefetchTask = null;
        _prefetchRepository = null;
    }

    // Forgets a prefetch that is being consumed by an actual menu open; the consumer owns its CTS.
    private void ClearPrefetch()
    {
        _prefetchCts = null;
        _prefetchTask = null;
        _prefetchRepository = null;
    }

    internal async Task InvokeDefaultActionOnSelectionAsync()
    {
        if (SelectedRepository is not RepositoryViewModel selectedRepository || !selectedRepository.WasFound)
        {
            return;
        }

        var skip = 0;
        if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            skip = 1;
        }

        UserInterfaceRepositoryActionBase uiRepositoryAction = await _userMenuActionMenuFactory
            .CreateMenuAsync(selectedRepository.Repository)
            .Skip(skip)
            .FirstAsync()
            .ConfigureAwait(false);

        if (uiRepositoryAction is not UserInterfaceRepositoryAction action || action.RepositoryCommand is NullRepositoryCommand)
        {
            return;
        }

        ExecuteRepositoryAction(action, selectedRepository);
    }

    internal void LogContextMenuError(Exception exception)
    {
        _logger.LogError(exception, "Could not create menu.");
    }

    internal void LogInvokeActionError(Exception exception)
    {
        _logger.LogError(exception, "Could not invoke action on current repository.");
    }

    internal void LogMenuPresentation(string repositoryPath, string menu, TimeSpan wait, TimeSpan populate)
    {
        _logger.LogInformation("Context menu presentation for {RepositoryPath}, {Menu}: wait={WaitMs:F1} ms; UI population={PopulateMs:F1} ms",
            repositoryPath, menu, wait.TotalMilliseconds, populate.TotalMilliseconds);
    }

    private IRepositoryMenuEntryViewModel? CreateMenuEntry(UserInterfaceRepositoryActionBase action, RepositoryViewModel selectedRepository, Guid buildId)
    {
        if (action is UserInterfaceSeparatorRepositoryAction)
        {
            return RepositoryMenuSeparatorViewModel.Instance;
        }

        if (action is not UserInterfaceRepositoryAction repositoryAction)
        {
            return null;
        }

        return new RepositoryMenuItemViewModel(
            repositoryAction.Name,
            repositoryAction.CanExecute,
            CreateExecuteAction(repositoryAction, selectedRepository),
            CreateSubMenuLoader(repositoryAction, selectedRepository, buildId));
    }

    private Action? CreateExecuteAction(UserInterfaceRepositoryAction action, RepositoryViewModel selectedRepository)
    {
        if (action.RepositoryCommand is null or NullRepositoryCommand)
        {
            return null;
        }

        return () => ExecuteRepositoryAction(action, selectedRepository);
    }

    private Func<Task<IReadOnlyList<IRepositoryMenuEntryViewModel>>>? CreateSubMenuLoader(
        UserInterfaceRepositoryAction repositoryAction,
        RepositoryViewModel selectedRepository,
        Guid buildId)
    {
        if (repositoryAction is DeferredSubActionsUserInterfaceRepositoryAction deferredRepositoryAction)
        {
            return async () =>
            {
                var timer = Stopwatch.StartNew();
                var outcome = "failed";
                try
                {
                    UserInterfaceRepositoryActionBase[] deferredSubActions = await deferredRepositoryAction.GetAsync().ConfigureAwait(false);
                    var entries = CreateMenuEntries(deferredSubActions, selectedRepository, buildId);
                    outcome = "completed";
                    return entries;
                }
                finally
                {
                    _logger.LogInformation("Context menu {MenuBuildId} deferred submenu {MenuItem} for {RepositoryPath}: {ElapsedMs:F1} ms; outcome={Outcome}",
                        buildId, repositoryAction.Name, selectedRepository.Path, timer.Elapsed.TotalMilliseconds, outcome);
                }
            };
        }

        if (repositoryAction.SubActions == null)
        {
            return null;
        }

        return () => Task.FromResult<IReadOnlyList<IRepositoryMenuEntryViewModel>>(CreateMenuEntries(repositoryAction.SubActions, selectedRepository, buildId));
    }

    private List<IRepositoryMenuEntryViewModel> CreateMenuEntries(
        IEnumerable<UserInterfaceRepositoryActionBase> actions,
        RepositoryViewModel selectedRepository,
        Guid buildId)
    {
        var entries = new List<IRepositoryMenuEntryViewModel>();
        foreach (UserInterfaceRepositoryActionBase action in actions)
        {
            IRepositoryMenuEntryViewModel? entry = CreateMenuEntry(action, selectedRepository, buildId);
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private void ExecuteRepositoryAction(UserInterfaceRepositoryAction action, RepositoryViewModel? selectedRepository)
    {
        Task.Run(() =>
        {
            try
            {
                if (action.ExecutionCausesSynchronizing)
                {
                    SetVmSynchronizing(selectedRepository, true);
                }

                _executor.Execute(action.Repository, action.RepositoryCommand);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Could not execute repository action {ActionName}.", action.Name);
            }
            finally
            {
                if (action.ExecutionCausesSynchronizing)
                {
                    SetVmSynchronizing(selectedRepository, false);
                }
            }
        });
    }

    private static void SetVmSynchronizing(RepositoryViewModel? selectedRepository, bool synchronizing)
    {
        if (selectedRepository != null)
        {
            selectedRepository.IsSynchronizing = synchronizing;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal interface IRepositoryMenuEntryViewModel;

internal sealed class RepositoryMenuSeparatorViewModel : IRepositoryMenuEntryViewModel
{
    public static RepositoryMenuSeparatorViewModel Instance { get; } = new();

    private RepositoryMenuSeparatorViewModel()
    {
    }
}

internal sealed class RepositoryMenuItemViewModel : IRepositoryMenuEntryViewModel
{
    private readonly Action? _execute;
    private readonly Lazy<Task<IReadOnlyList<IRepositoryMenuEntryViewModel>>>? _childrenTask;

    public RepositoryMenuItemViewModel(
        string header,
        bool isEnabled,
        Action? execute,
        Func<Task<IReadOnlyList<IRepositoryMenuEntryViewModel>>>? loadChildrenAsync)
    {
        Header = header ?? throw new ArgumentNullException(nameof(header));
        IsEnabled = isEnabled;
        _execute = execute;
        if (loadChildrenAsync != null)
        {
            _childrenTask = new Lazy<Task<IReadOnlyList<IRepositoryMenuEntryViewModel>>>(
                () => Task.Run(loadChildrenAsync), LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    public string Header { get; }

    public bool IsEnabled { get; }

    public bool HasSubItems => _childrenTask != null;

    public void Execute()
    {
        _execute?.Invoke();
    }

    // Memoized so a prefetch and the subsequent real open share the same (single) load.
    public Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> LoadChildrenAsync()
    {
        return _childrenTask?.Value ?? Task.FromResult<IReadOnlyList<IRepositoryMenuEntryViewModel>>([]);
    }
}