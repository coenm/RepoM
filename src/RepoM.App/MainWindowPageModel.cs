namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;
using RepoM.App.ViewModels;


public class SortMenuItemViewModel : MenuItemViewModel
{
    private readonly Func<bool> _isSelectedFunc;
    private readonly Action _setKeyFunc;

    public SortMenuItemViewModel(
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
}

public class OrderingsViewModel : List<MenuItemViewModel>
{
    private readonly IRepositoryComparerManager _repositoryComparerManager;
    private readonly IAppSettingsService _appSettingsService;

    public OrderingsViewModel(
        IRepositoryComparerManager repositoryComparerManager,
        IAppSettingsService appSettingsService)
    {
        _repositoryComparerManager = repositoryComparerManager ?? throw new ArgumentNullException(nameof(repositoryComparerManager));


        _repositoryComparerManager.SelectedRepositoryComparerKeyChanged += (sender, key) =>
            {

            };
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));

        AddRange(
            _repositoryComparerManager
                .RepositoryComparerKeys
                .Select(name => new SortMenuItemViewModel(
                    () => _repositoryComparerManager.SelectedRepositoryComparerKey == name,
                    () => _repositoryComparerManager.SetRepositoryComparer(name),
                    name)));


    // public AutoFetchMode AutoFetchMode
    // {
    //     get => _appSettingsService.AutoFetchMode;
    //     set
    //     {
    //         _appSettingsService.AutoFetchMode = value;
    //
    //         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchMode)));
    //         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchOff)));
    //         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchDiscretely)));
    //         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchAdequate)));
    //         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoFetchAggressive)));
    //         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnabledSearchRepoEverything)));
    //     }
    // }
    // };
}
}

public class MainWindowPageModel : INotifyPropertyChanged
{
    private readonly IAppSettingsService _appSettingsService;
    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindowPageModel(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        Orderings = new OrderingsViewModel(_appSettingsService);
    }

    public OrderingsViewModel Orderings { get; }
    
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