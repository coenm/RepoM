namespace RepoM.Plugin.Misc.Tests.TestFramework.AssemblyAndTypeHelpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal static class AssemblyHelpers
{
    public static Type[] GetRepositoryActionsFromAssembly(this Assembly assembly)
    {
        return assembly.GetExportedTypesFrom()
                       .Where(t => typeof(Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction).IsAssignableFrom(t))
                       .Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false, })
                       .ToArray();
    }

    public static IEnumerable<Type> GetExportedTypesFrom(this Assembly assembly)
    {
        try
        {
            return assembly.DefinedTypes.Select(info => info.AsType());
        }
        catch (NotSupportedException)
        {
            // A type load exception would typically happen on an Anonymously Hosted DynamicMethods
            // Assembly and it would be safe to skip this exception.
            return Enumerable.Empty<Type>();
        }
    }
}