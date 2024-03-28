namespace RepoM.Plugin.Misc.Tests;

using System;
using System.Collections.Generic;
using System.Reflection;

internal static class RepoMAssemblyStore
{
    public static IEnumerable<Assembly> GetAssemblies()
    {
        List<Assembly> results = new();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var assemblyName = assembly.GetName().Name;
                if (string.IsNullOrEmpty(assemblyName) || !assemblyName.Contains("RepoM"))
                {
                    continue;
                }

                // Workaround for Github Actions
                if (assemblyName.Contains("Test"))
                {
                    continue;
                }

                results.Add(assembly);
            }
            catch (Exception)
            {
                // skip
            }
        }

        // tmp, not sure why it is someties forgotten.
        Assembly a = typeof(RepoM.Api.CoreBootstrapper).Assembly;
        if (!results.Contains(a))
        {
            results.Add(a);
        }

        return results;
    }
}