namespace RepoM.App.Plugins;

using System.Collections.Generic;
using System.IO.Abstractions;

internal interface IPluginFinder
{
    IEnumerable<PluginInfo> FindPlugins(string baseDirectory);

    // IEnumerable<IFileInfo> FindPluginAssemblies(string baseDirectory);
}