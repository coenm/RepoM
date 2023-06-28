namespace RepoM.App.Plugins;

using System;
using System.Collections.Generic;

public interface IModuleManager
{
    event EventHandler? Changed;

    IReadOnlyList<PluginModel> Plugins { get; }
}