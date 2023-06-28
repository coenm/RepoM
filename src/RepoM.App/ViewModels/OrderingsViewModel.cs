namespace RepoM.App.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.App.Plugins;
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

public class PluginsViewModel : List<MenuItemViewModel>
{
    public PluginsViewModel(IModuleManager moduleManager, IThreadDispatcher threadDispatcher)
    {
        if (moduleManager == null)
        {
            throw new ArgumentNullException(nameof(moduleManager));
        }

        if (threadDispatcher == null)
        {
            throw new ArgumentNullException(nameof(threadDispatcher));
        }

        // moduleManager.SelectedRepositoryComparerKeyChanged += (_, _) =>
        //     {
        //         foreach (MenuItemViewModel item in this)
        //         {
        //             if (item is ActionCheckableMenuItemViewModel sortMenuItemViewModel)
        //             {
        //                 threadDispatcher.Invoke(() => sortMenuItemViewModel.Poke());
        //             }
        //         }
        //     };

        AddRange(moduleManager.Plugins.Select(plugin => new PluginViewModel(plugin)));
    }
}

public class PluginViewModel : MenuItemViewModel
{
    private readonly PluginModel _model;

    public PluginViewModel(PluginModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public override bool IsChecked
    {
        get => _model.Enabled;
        set => _model.Enabled = value;
    }
}