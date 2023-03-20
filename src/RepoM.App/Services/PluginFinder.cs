namespace RepoM.App.Services;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

internal static class PluginFinder
{
    public static IEnumerable<FileInfo> FindPluginAssemblies(string baseDirectory, IFileSystem fileSystem)
    {
        IEnumerable<FileInfo> assemblies = GetPluginAssembliesInDirectory(baseDirectory);

        foreach (var dir in GetPluginDirectories(baseDirectory, fileSystem))
        {
            assemblies = assemblies.Concat(GetPluginAssembliesInDirectory(dir));
        }

        return assemblies;
    }

    private static IEnumerable<string> GetPluginDirectories(string baseDirectory, IFileSystem fileSystem)
    {
        var pluginBaseDirectory = Path.Combine(baseDirectory, "Plugins");

        if (!fileSystem.Directory.Exists(pluginBaseDirectory))
        {
            return Enumerable.Empty<string>();
        }

        return fileSystem.Directory.EnumerateDirectories(pluginBaseDirectory);
    }

    private static IEnumerable<FileInfo> GetPluginAssembliesInDirectory(string baseDirectory)
    {
        // todo IFileystem
        return new DirectoryInfo(baseDirectory)
            .GetFiles()
            .Where(file =>
                file.Name.StartsWith("RepoM.Plugin.")
                &&
                file.Extension.ToLower() == ".dll");
            // .Select(file => Assembly.Load(AssemblyName.GetAssemblyName(file.FullName)));
    }
}