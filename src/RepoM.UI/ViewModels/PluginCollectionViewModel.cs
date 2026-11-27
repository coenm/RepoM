namespace RepoM.UI.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.UI.Plugins;

public class PluginCollectionViewModel : List<MenuItemViewModel>
{
    public PluginCollectionViewModel(IModuleManager moduleManager)
    {
        ArgumentNullException.ThrowIfNull(moduleManager);

        AddRange(moduleManager.Plugins.Select(plugin => new PluginViewModel(plugin)));
    }
}