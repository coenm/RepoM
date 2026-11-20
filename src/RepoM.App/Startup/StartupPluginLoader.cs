namespace RepoM.App.Startup;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RepoM.Core.StartupPlugin;

internal static class StartupPluginLoader
{
    public static IStartupPlugin Load(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!string.Equals(Path.GetExtension(fullPath), ".dll", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A startup plugin must be a DLL.", nameof(path));
        }

        // LoadFrom keeps shared contracts in the default context and probes the plugin's directory.
        Assembly assembly = Assembly.LoadFrom(fullPath);
        Type[] candidates = assembly.GetExportedTypes()
            .Where(type => typeof(IStartupPlugin).IsAssignableFrom(type)
                && type is { IsClass: true, IsAbstract: false, ContainsGenericParameters: false })
            .ToArray();
        if (candidates.Length != 1 || candidates[0].GetConstructor(Type.EmptyTypes) == null)
        {
            throw new InvalidOperationException(
                $"'{fullPath}' must contain exactly one public IStartupPlugin with a public parameterless constructor.");
        }

        return (IStartupPlugin)Activator.CreateInstance(candidates[0])!;
    }
}
