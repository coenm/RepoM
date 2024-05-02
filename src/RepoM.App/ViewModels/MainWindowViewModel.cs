namespace RepoM.App.ViewModels;

using System;
using System.ComponentModel;
using JetBrains.Annotations;
using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IAppSettingsService _appSettingsService;
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindowViewModel(
        IAppSettingsService appSettingsService,
        OrderingsViewModel orderingsViewModel,
        QueryParsersViewModel queryParsersViewModel,
        FiltersViewModel filtersViewModel,
        PluginCollectionViewModel pluginsViewModel,
        HelpViewModel helpViewModel)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        Orderings = orderingsViewModel ?? throw new ArgumentNullException(nameof(orderingsViewModel));
        QueryParsers = queryParsersViewModel ?? throw new ArgumentNullException(nameof(queryParsersViewModel));
        Filters = filtersViewModel ?? throw new ArgumentNullException(nameof(filtersViewModel));
        Plugins = pluginsViewModel ?? throw new ArgumentNullException(nameof(pluginsViewModel));
        Help = helpViewModel ?? throw new ArgumentNullException(nameof(helpViewModel));
    }

    private AutoFetchMode AutoFetchMode
    {
        get => _appSettingsService.AutoFetchMode;
        set
        {
            _appSettingsService.AutoFetchMode = value;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchMode)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchOff)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchDiscretely)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchAdequate)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchAggressive)));
        }
    }

    public QueryParsersViewModel QueryParsers { [UsedImplicitly] get; }

    public OrderingsViewModel Orderings { [UsedImplicitly] get; }

    public FiltersViewModel Filters { [UsedImplicitly] get; }

    public PluginCollectionViewModel Plugins { [UsedImplicitly] get; }

    public HelpViewModel Help { [UsedImplicitly] get; }

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
}