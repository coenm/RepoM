namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Lucene.Net.Util.Packed;
using Microsoft.Xaml.Behaviors.Core;
using RepoM.Api.Common;
using RepoM.Api.Git.AutoFetch;
using RepoM.App.ViewModels;


public class SortMenuItemViewModel : MenuItemViewModel
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly string _title;

    public SortMenuItemViewModel(
        IAppSettingsService appSettingsService,
        string title)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _title = title;

        Header = title;
        Command = new DelegateCommand
            {
                CommandAction = () =>
                    {
                        _appSettingsService.SortKey = title;
                    },
                CanExecuteFunc = () => _appSettingsService.SortKey != title,
            };
        IsCheckable = true;
        IsChecked = _appSettingsService.SortKey == title;
    }

    public override bool IsChecked
    {
        get => _appSettingsService.SortKey == _title;
        set => Command?.Execute(null);
    }
}

public class OrderingsViewModel : List<MenuItemViewModel>
{
    private readonly IAppSettingsService _appSettingsService;

    public OrderingsViewModel(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));

        Add(new SortMenuItemViewModel(_appSettingsService, "DRC First")
        {
                Header = "DRC first",
                IsCheckable = false,
                IsChecked = false,
                Command = new DelegateCommand
                    {
                        CommandAction = () => _appSettingsService.SortKey = "",
                        CanExecuteFunc = () => _appSettingsService.SortKey != "",
                    }
            });
        Add(new()
            {
                Header = "Private 1",
                IsCheckable = true,
                IsChecked = true,
            });
        Add(new()
            {
                Header = "Private 2",
                IsCheckable = true,
                IsChecked = false,
            });


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