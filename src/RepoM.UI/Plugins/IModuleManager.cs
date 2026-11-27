namespace RepoM.UI.Plugins;

using System.Collections.Generic;

public interface IModuleManager
{
    IReadOnlyList<PluginModel> Plugins { get; }
}