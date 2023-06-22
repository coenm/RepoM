namespace RepoM.App.Plugins;

using System.Collections.Generic;

internal interface IPluginFinder
{
    IEnumerable<PluginInfo> FindPlugins(string baseDirectory);
}