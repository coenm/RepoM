namespace RepoM.Api.Plugins;

using System.Collections.Generic;

public interface IPluginFinder
{
    IEnumerable<PluginInfo> FindPlugins(string baseDirectory);
}