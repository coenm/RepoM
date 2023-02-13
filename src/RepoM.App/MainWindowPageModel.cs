namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryOrdering;
using RepoM.App.ViewModels;

public class ActionCheckableMenuItemViewModel : MenuItemViewModel
{
    private readonly Func<bool> _isSelectedFunc;
    private readonly Action _setKeyFunc;

    public ActionCheckableMenuItemViewModel(
        Func<bool> isSelectedFunc,
        Action setKeyFunc,
        string title)
    {
        _isSelectedFunc = isSelectedFunc ?? throw new ArgumentNullException(nameof(isSelectedFunc));
        _setKeyFunc = setKeyFunc ?? throw new ArgumentNullException(nameof(setKeyFunc));

        Header = title;
        IsCheckable = true;
    }

    public override bool IsChecked
    {
        get => _isSelectedFunc.Invoke();
        set => _setKeyFunc.Invoke();
    }

    public void Poke()
    {
        OnPropertyChanged(nameof(IsChecked));
    }
}

public class QueryParsersViewModel : List<MenuItemViewModel>
{
    public QueryParsersViewModel(IRepositoryFilteringManager repositoryFilterManager, IThreadDispatcher threadDispatcher)
    {
        if (repositoryFilterManager == null)
        {
            throw new ArgumentNullException(nameof(repositoryFilterManager));
        }

        if (threadDispatcher == null)
        {
            throw new ArgumentNullException(nameof(threadDispatcher));
        }

        repositoryFilterManager.SelectedQueryParserChanged += (_, _) =>
            {
                foreach (MenuItemViewModel item in this)
                {
                    if (item is ActionCheckableMenuItemViewModel vm)
                    {
                        threadDispatcher.Invoke(() => vm.Poke());
                    }
                }
            };

        AddRange(repositoryFilterManager.QueryParserKeys.Select(name =>
            new ActionCheckableMenuItemViewModel(
                () => repositoryFilterManager.SelectedQueryParserKey == name,
                () => repositoryFilterManager.SetQueryParser(name),
                name)));
    }
}

public class FiltersViewModel : List<MenuItemViewModel>
{
    public FiltersViewModel(IRepositoryFilteringManager repositoryFilterManager, IThreadDispatcher threadDispatcher)
    {
        if (repositoryFilterManager == null)
        {
            throw new ArgumentNullException(nameof(repositoryFilterManager));
        }

        if (threadDispatcher == null)
        {
            throw new ArgumentNullException(nameof(threadDispatcher));
        }

        repositoryFilterManager.SelectedFilterChanged += (_, _) =>
            {
                foreach (MenuItemViewModel item in this)
                {
                    if (item is ActionCheckableMenuItemViewModel vm)
                    {
                        threadDispatcher.Invoke(() => vm.Poke());
                    }
                }
            };

        AddRange(repositoryFilterManager.FilterKeys.Select(name =>
            new ActionCheckableMenuItemViewModel(
                () => repositoryFilterManager.SelectedFilterKey == name,
                () => repositoryFilterManager.SetFilter(name),
                name)));
    }
}

public class OrderingsViewModel : List<MenuItemViewModel>
{
    public OrderingsViewModel(IRepositoryComparerManager repositoryComparerManager, IThreadDispatcher threadDispatcher)
    {
        if (repositoryComparerManager == null)
        {
            throw new ArgumentNullException(nameof(repositoryComparerManager));
        }

        if (threadDispatcher == null)
        {
            throw new ArgumentNullException(nameof(threadDispatcher));
        }

        repositoryComparerManager.SelectedRepositoryComparerKeyChanged += (_, _) =>
            {
                foreach (MenuItemViewModel item in this)
                {
                    if (item is ActionCheckableMenuItemViewModel sortMenuItemViewModel)
                    {
                        threadDispatcher.Invoke(() => sortMenuItemViewModel.Poke());
                    }
                }
            };

        AddRange(repositoryComparerManager.RepositoryComparerKeys.Select(name =>
            new ActionCheckableMenuItemViewModel(
                () => repositoryComparerManager.SelectedRepositoryComparerKey == name,
                () => repositoryComparerManager.SetRepositoryComparer(name),
                name)));
    }
}

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