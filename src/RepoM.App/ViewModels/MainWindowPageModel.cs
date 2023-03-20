namespace RepoM.App.ViewModels;

using System;
using System.ComponentModel;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;

public class MainWindowPageModel : INotifyPropertyChanged
{
    private readonly IAppSettingsService _appSettingsService;
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindowPageModel(
        IAppSettingsService appSettingsService,
        OrderingsViewModel orderingsViewModel,
        QueryParsersViewModel queryParsersViewModel,
        FiltersViewModel filtersViewModel)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        Orderings = orderingsViewModel ?? throw new ArgumentNullException(nameof(orderingsViewModel));
        QueryParsers = queryParsersViewModel ?? throw new ArgumentNullException(nameof(queryParsersViewModel));
        Filters = filtersViewModel ?? throw new ArgumentNullException(nameof(filtersViewModel));
    }

    public QueryParsersViewModel QueryParsers { get; }

    public OrderingsViewModel Orderings { get; }

    public FiltersViewModel Filters { get; }

    public AutoFetchMode AutoFetchMode
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledSearchRepoEverything)));
        }
    }

    public bool AutoFetchOff
    {
        get => AutoFetchMode == AutoFetchMode.Off;
        set => AutoFetchMode = AutoFetchMode.Off;
    }

    public bool AutoFetchDiscretely
    {
        get => AutoFetchMode == AutoFetchMode.Discretely;
        set => AutoFetchMode = AutoFetchMode.Discretely;
    }

    public bool AutoFetchAdequate
    {
        get => AutoFetchMode == AutoFetchMode.Adequate;
        set => AutoFetchMode = AutoFetchMode.Adequate;
    }

    public bool AutoFetchAggressive
    {
        get => AutoFetchMode == AutoFetchMode.Aggressive;
        set => AutoFetchMode = AutoFetchMode.Aggressive;
    }

    public bool PruneOnFetch
    {
        get => _appSettingsService.PruneOnFetch;
        set => _appSettingsService.PruneOnFetch = value;
    }

    public bool EnabledSearchRepoEverything
    {
        get => _appSettingsService.EnabledSearchProviders.Any(item => "Everything".Equals(item, StringComparison.CurrentCultureIgnoreCase));
        set
        {
            if (value)
            {
                if (EnabledSearchRepoEverything)
                {
                    return;
                }

                var list = _appSettingsService.EnabledSearchProviders.ToList();
                list.Add("Everything");
                _appSettingsService.EnabledSearchProviders = list;
            }
            else
            {
                if (!EnabledSearchRepoEverything)
                {
                    return;
                }

                var list = _appSettingsService.EnabledSearchProviders.ToList();
                var count = list.RemoveAll(item => "Everything".Equals(item, StringComparison.CurrentCultureIgnoreCase));
                if (count > 0)
                {
                    _appSettingsService.EnabledSearchProviders = list;
                }
            }
        }
    }
}