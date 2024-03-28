namespace RepoM.ActionMenu.Interface.YamlModel;

using System;
using JetBrains.Annotations;

/// <summary>
/// Attribute the textual type of the repository action.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public sealed class RepositoryActionAttribute : Attribute
{
    public RepositoryActionAttribute(string type)
    {
        Type = type;
    }

    public string Type { get; }
}