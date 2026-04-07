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

    internal async Task<IReadOnlyList<RepositoryMenuEntryViewModel>> CreateContextMenuEntriesAsync(CancellationToken cancellationToken)
    {
        if (SelectedRepository is not RepositoryViewModel selectedRepository)
        {
            return [];
        }

        selectedRepository.EnableMonitoring();

        RepositoryInfo? updatedInfo = await _monitorService.RefreshRepositoryAsync(selectedRepository.Path, cancellationToken).ConfigureAwait(false);
        IRepository repositoryForMenu = updatedInfo != null ? new RepositoryInfoAdapter(updatedInfo) : selectedRepository.Repository;

        var entries = new List<RepositoryMenuEntryViewModel>();
        await foreach (UserInterfaceRepositoryActionBase action in _userMenuActionMenuFactory.CreateMenuAsync(repositoryForMenu).ConfigureAwait(false))
        {
            RepositoryMenuEntryViewModel? entry = CreateMenuEntry(action, selectedRepository);
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        return entries;
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

    private RepositoryMenuEntryViewModel? CreateMenuEntry(UserInterfaceRepositoryActionBase action, RepositoryViewModel selectedRepository)
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

    private Func<Task<IReadOnlyList<RepositoryMenuEntryViewModel>>>? CreateSubMenuLoader(
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

        return () => Task.FromResult<IReadOnlyList<RepositoryMenuEntryViewModel>>(CreateMenuEntries(repositoryAction.SubActions, selectedRepository));
    }

    private List<RepositoryMenuEntryViewModel> CreateMenuEntries(
        IEnumerable<UserInterfaceRepositoryActionBase> actions,
        RepositoryViewModel selectedRepository)
    {
        var entries = new List<RepositoryMenuEntryViewModel>();
        foreach (UserInterfaceRepositoryActionBase action in actions)
        {
            RepositoryMenuEntryViewModel? entry = CreateMenuEntry(action, selectedRepository);
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

internal abstract class RepositoryMenuEntryViewModel;

internal sealed class RepositoryMenuSeparatorViewModel : RepositoryMenuEntryViewModel
{
    public static RepositoryMenuSeparatorViewModel Instance { get; } = new();

    private RepositoryMenuSeparatorViewModel()
    {
    }
}

internal sealed class RepositoryMenuItemViewModel : RepositoryMenuEntryViewModel
{
    private readonly Action? _execute;
    private readonly Func<Task<IReadOnlyList<RepositoryMenuEntryViewModel>>>? _loadChildrenAsync;

    public RepositoryMenuItemViewModel(
        string header,
        bool isEnabled,
        Action? execute,
        Func<Task<IReadOnlyList<RepositoryMenuEntryViewModel>>>? loadChildrenAsync)
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

    public Task<IReadOnlyList<RepositoryMenuEntryViewModel>> LoadChildrenAsync()
    {
        return _loadChildrenAsync?.Invoke() ?? Task.FromResult<IReadOnlyList<RepositoryMenuEntryViewModel>>([]);
    }
}