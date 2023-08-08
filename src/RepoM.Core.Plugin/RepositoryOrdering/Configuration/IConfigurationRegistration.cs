namespace RepoM.Core.Plugin.RepositoryOrdering.Configuration;

using System;

/// <summary>
/// Configuration registration per name
/// </summary>
public interface IConfigurationRegistration
{
    public Type ConfigurationType { get; }

    public string Tag { get; }
}

public interface IKeyTypeRegistration<T> : IConfigurationRegistration   
{
}