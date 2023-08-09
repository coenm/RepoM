namespace RepoM.Core.Plugin.RepositoryOrdering.Configuration;

using System;

/// <summary>
/// Configuration registration per name
/// </summary>
public interface IConfigurationRegistration : IKeyTypeRegistration<object>
{
}

/// <summary>
/// Configuration registration per name
/// </summary>
public interface IKeyTypeRegistration<T>   
{
    public Type ConfigurationType { get; }

    public string Tag { get; }
}