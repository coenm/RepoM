namespace RepoM.App.ViewModels;

using System;
using System.ComponentModel;
using System.Windows.Input;
using JetBrains.Annotations;
using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs _autoFetchModeChangedArgs = new(nameof(AutoFetchMode));
    private static readonly PropertyChangedEventArgs _autoFetchOffChangedArgs = new(nameof(AutoFetchOff));
    private static readonly PropertyChangedEventArgs _autoFetchDiscretelyChangedArgs = new(nameof(AutoFetchDiscretely));
    private static readonly PropertyChangedEventArgs _autoFetchAdequateChangedArgs = new(nameof(AutoFetchAdequate));
    private static readonly PropertyChangedEventArgs _autoFetchAggressiveChangedArgs = new(nameof(AutoFetchAggressive));

    private readonly IAppSettingsService _appSettingsService;
    private static readonly ICommand _noOpCommand = new NoOpCommand();
    private static readonly MainWindowQuickFilterCommands _noOpQuickFilterCommands = new(_noOpCommand, _noOpCommand);

    public event PropertyChangedEventHandler? PropertyChanged;

    internal MainWindowViewModel(
        IAppSettingsService appSettingsService,
        OrderingsViewModel orderingsViewModel,
        QueryParsersViewModel queryParsersViewModel,
        FiltersViewModel filtersViewModel,
        PluginCollectionViewModel pluginsViewModel,
        HelpViewModel helpViewModel)
        : this(
            appSettingsService,
            orderingsViewModel,
            queryParsersViewModel,
            filtersViewModel,
            pluginsViewModel,
            helpViewModel,
                _noOpQuickFilterCommands)
    {
    }

    internal MainWindowViewModel(
        IAppSettingsService appSettingsService,
        OrderingsViewModel orderingsViewModel,
        QueryParsersViewModel queryParsersViewModel,
        FiltersViewModel filtersViewModel,
        PluginCollectionViewModel pluginsViewModel,
        HelpViewModel helpViewModel,
        MainWindowQuickFilterCommands quickFilterCommands)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        Orderings = orderingsViewModel ?? throw new ArgumentNullException(nameof(orderingsViewModel));
        QueryParsers = queryParsersViewModel ?? throw new ArgumentNullException(nameof(queryParsersViewModel));
        Filters = filtersViewModel ?? throw new ArgumentNullException(nameof(filtersViewModel));
        Plugins = pluginsViewModel ?? throw new ArgumentNullException(nameof(pluginsViewModel));
        Help = helpViewModel ?? throw new ArgumentNullException(nameof(helpViewModel));
        ArgumentNullException.ThrowIfNull(quickFilterCommands);
        SaveQuickFilterCommand = quickFilterCommands.SaveQuickFilterCommand;
        AddQuickFilterTagCommand = quickFilterCommands.AddQuickFilterTagCommand;
    }

    private AutoFetchMode AutoFetchMode
    {
        get => _appSettingsService.AutoFetchMode;
        set
        {
            _appSettingsService.AutoFetchMode = value;

            PropertyChanged?.Invoke(this, _autoFetchModeChangedArgs);
            PropertyChanged?.Invoke(this, _autoFetchOffChangedArgs);
            PropertyChanged?.Invoke(this, _autoFetchDiscretelyChangedArgs);
            PropertyChanged?.Invoke(this, _autoFetchAdequateChangedArgs);
            PropertyChanged?.Invoke(this, _autoFetchAggressiveChangedArgs);
        }
    }

    public QueryParsersViewModel QueryParsers { [UsedImplicitly] get; }

    public OrderingsViewModel Orderings { [UsedImplicitly] get; }

    public FiltersViewModel Filters { [UsedImplicitly] get; }

    public PluginCollectionViewModel Plugins { [UsedImplicitly] get; }

    public HelpViewModel Help { [UsedImplicitly] get; }

    public ICommand SaveQuickFilterCommand { [UsedImplicitly] get; }

    public ICommand AddQuickFilterTagCommand { [UsedImplicitly] get; }

    public bool AutoFetchOff
    {
        get => AutoFetchMode == AutoFetchMode.Off;
        set
        {
            _ = value; // avoid warnings to use 'value' in setter.
            AutoFetchMode = AutoFetchMode.Off;
        }
    }

    public bool AutoFetchDiscretely
    {
        get => AutoFetchMode == AutoFetchMode.Discretely;
        set
        {
            _ = value; // avoid warnings to use 'value' in setter.
            AutoFetchMode = AutoFetchMode.Discretely;
        }
    }

    public bool AutoFetchAdequate
    {
        get => AutoFetchMode == AutoFetchMode.Adequate;
        set
        {
            _ = value; // avoid warnings to use 'value' in setter.
            AutoFetchMode = AutoFetchMode.Adequate;
        }
    }

    public bool AutoFetchAggressive
    {
        get => AutoFetchMode == AutoFetchMode.Aggressive;
        set
        {
            _ = value; // avoid warnings to use 'value' in setter.
            AutoFetchMode = AutoFetchMode.Aggressive;
        }
    }

    public bool PruneOnFetch
    {
        get => _appSettingsService.PruneOnFetch;
        set => _appSettingsService.PruneOnFetch = value;
    }

    private sealed class NoOpCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add => _ = value;
            remove => _ = value;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _ = parameter;
    }
}