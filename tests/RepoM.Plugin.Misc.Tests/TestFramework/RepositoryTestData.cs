namespace RepoM.Plugin.Misc.Tests.TestFramework;

using System;
using System.Reflection;

/// <summary>
/// Helper class used for naming arguments in xunits test name generation.
/// </summary>
public class RepositoryTestData
{
    public RepositoryTestData(Assembly assembly, Type type)
    {
        Assembly = assembly;
        Type = type;
    }

    public Assembly Assembly { get; }

    public Type Type { get; }

    public override string ToString()
    {
        return Assembly.GetName().Name + "-" + Type.Name;
    }
}