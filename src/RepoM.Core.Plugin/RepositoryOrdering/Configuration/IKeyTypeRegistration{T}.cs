namespace RepoM.Core.Plugin.RepositoryOrdering.Configuration;

using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Configuration registration per name
/// </summary>
[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "Used for registrations in DI container.")]
public interface IKeyTypeRegistration<T>   
{
    public Type ConfigurationType { get; }

    public string Tag { get; }
}