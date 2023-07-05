namespace RepoM.App.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.App.RepositoryOrdering;

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