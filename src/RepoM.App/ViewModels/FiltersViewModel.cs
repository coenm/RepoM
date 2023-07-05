namespace RepoM.App.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.App.RepositoryFiltering;

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