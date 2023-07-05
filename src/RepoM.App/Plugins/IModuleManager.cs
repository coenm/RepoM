namespace RepoM.App.Plugins;

using System.Collections.Generic;

public interface IModuleManager
{
    IReadOnlyList<PluginModel> Plugins { get; }
}