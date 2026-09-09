namespace RepoM.App.ViewModels;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.Git;
using RepoM.App.RepositoryActions;
using RepoM.App.Services;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Model;

public sealed class RepositoryListViewModel : INotifyPropertyChanged
{
    private readonly RepositoryMonitorService _monitorService;
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
        RepositoryMonitorService monitorService,
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
    public TimeSpan MenuPrefetchHoverDelay { get; set; } = TimeSpan.FromSeconds(2);

    internal Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> CreateContextMenuEntriesAsync(CancellationToken cancellationToken)
    {
        if (SelectedRepository is not RepositoryViewModel selectedRepository)
        {
            return Task.FromResult<IReadOnlyList<IRepositoryMenuEntryViewModel>>([]);
        }

        // Reuse the background prefetch (started on hover) when it targets the repository being opened.
        lock (_prefetchLock)
        {
            if (_prefetchTask is not null && ReferenceEquals(_prefetchRepository, selectedRepository))
            {
                Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> prefetched = _prefetchTask;
                ClearPrefetch();
                return prefetched;
            }
        }

        return BuildContextMenuEntriesAsync(selectedRepository, cancellationToken);
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
            _prefetchTask = PrefetchAsync(repository, cts.Token);
        }
    }

    private async Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> PrefetchAsync(RepositoryViewModel repository, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<IRepositoryMenuEntryViewModel> entries = await BuildContextMenuEntriesAsync(repository, cancellationToken).ConfigureAwait(false);

            // Also load submenus ahead of time so they open instantly too.
            await PreloadSubMenusAsync(entries, cancellationToken).ConfigureAwait(false);

            return entries;
        }
        catch (OperationCanceledException)
        {
            return [];
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Context menu prefetch failed.");
            return [];
        }
    }

    private async Task PreloadSubMenusAsync(IReadOnlyList<IRepositoryMenuEntryViewModel> entries, CancellationToken cancellationToken)
    {
        foreach (IRepositoryMenuEntryViewModel entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry is RepositoryMenuItemViewModel { HasSubItems: true } item)
            {
                IReadOnlyList<IRepositoryMenuEntryViewModel> children = await item.LoadChildrenAsync().ConfigureAwait(false);
                await PreloadSubMenusAsync(children, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> BuildContextMenuEntriesAsync(RepositoryViewModel selectedRepository, CancellationToken cancellationToken)
    {
        selectedRepository.EnableMonitoring();

        RepositoryInfo? updatedInfo = await _monitorService.RefreshRepositoryAsync(selectedRepository.Path, cancellationToken).ConfigureAwait(false);
        IRepository repositoryForMenu = updatedInfo != null ? new RepositoryInfoAdapter(updatedInfo) : selectedRepository.Repository;

        var entries = new List<IRepositoryMenuEntryViewModel>();
        await foreach (UserInterfaceRepositoryActionBase action in _userMenuActionMenuFactory.CreateMenuAsync(repositoryForMenu).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            IRepositoryMenuEntryViewModel? entry = CreateMenuEntry(action, selectedRepository);
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        return entries;
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

    // Forgets a prefetch that is being consumed by an actual menu open, without cancelling it.
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

    private IRepositoryMenuEntryViewModel? CreateMenuEntry(UserInterfaceRepositoryActionBase action, RepositoryViewModel selectedRepository)
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
            CreateSubMenuLoader(repositoryAction, selectedRepository));
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
        RepositoryViewModel selectedRepository)
    {
        if (repositoryAction is DeferredSubActionsUserInterfaceRepositoryAction deferredRepositoryAction)
        {
            return async () =>
            {
                UserInterfaceRepositoryActionBase[] deferredSubActions = await deferredRepositoryAction.GetAsync().ConfigureAwait(false);
                return CreateMenuEntries(deferredSubActions, selectedRepository);
            };
        }

        if (repositoryAction.SubActions == null)
        {
            return null;
        }

        return () => Task.FromResult<IReadOnlyList<IRepositoryMenuEntryViewModel>>(CreateMenuEntries(repositoryAction.SubActions, selectedRepository));
    }

    private List<IRepositoryMenuEntryViewModel> CreateMenuEntries(
        IEnumerable<UserInterfaceRepositoryActionBase> actions,
        RepositoryViewModel selectedRepository)
    {
        var entries = new List<IRepositoryMenuEntryViewModel>();
        foreach (UserInterfaceRepositoryActionBase action in actions)
        {
            IRepositoryMenuEntryViewModel? entry = CreateMenuEntry(action, selectedRepository);
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
    private readonly Func<Task<IReadOnlyList<IRepositoryMenuEntryViewModel>>>? _loadChildrenAsync;
    private Task<IReadOnlyList<IRepositoryMenuEntryViewModel>>? _childrenTask;

    public RepositoryMenuItemViewModel(
        string header,
        bool isEnabled,
        Action? execute,
        Func<Task<IReadOnlyList<IRepositoryMenuEntryViewModel>>>? loadChildrenAsync)
    {
        Header = header ?? throw new ArgumentNullException(nameof(header));
        IsEnabled = isEnabled;
        _execute = execute;
        _loadChildrenAsync = loadChildrenAsync;
    }

    public string Header { get; }

    public bool IsEnabled { get; }

    public bool HasSubItems => _loadChildrenAsync != null;

    public void Execute()
    {
        _execute?.Invoke();
    }

    // Memoized so a prefetch and the subsequent real open share the same (single) load.
    public Task<IReadOnlyList<IRepositoryMenuEntryViewModel>> LoadChildrenAsync()
    {
        return _childrenTask ??= _loadChildrenAsync?.Invoke() ?? Task.FromResult<IReadOnlyList<IRepositoryMenuEntryViewModel>>([]);
    }
}