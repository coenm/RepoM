namespace RepoM.UI.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.UI.RepositoryFiltering;

public class QueryParsersViewModel : List<MenuItemViewModel>
{
    public QueryParsersViewModel(IRepositoryFilteringManager repositoryFilterManager, IThreadDispatcher threadDispatcher)
    {
        ArgumentNullException.ThrowIfNull(repositoryFilterManager);
        ArgumentNullException.ThrowIfNull(threadDispatcher);

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