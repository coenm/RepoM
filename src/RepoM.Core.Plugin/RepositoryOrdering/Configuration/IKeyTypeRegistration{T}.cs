namespace RepoM.Core.Plugin.RepositoryOrdering.Configuration;

using System;
using System.Diagnostics;

/// <summary>
/// Configuration registration per name
/// </summary>
// ReSharper disable once UnusedTypeParameter
public interface IKeyTypeRegistration<T>   
{
    public Type ConfigurationType { get; }

    public string Tag { get; }
}

[DebuggerDisplay($"{{{nameof(Tag)}}}")]
public sealed class FixedTypeRegistration<T> : IKeyTypeRegistration<T>
{
    public FixedTypeRegistration(Type configurationType, string tag)
    {
        ConfigurationType = configurationType;

        if (string.IsNullOrEmpty(tag))
        {
            throw new ArgumentNullException(nameof(tag));
        }

        Tag = tag;
    }

    public Type ConfigurationType { get; }

    public string Tag { get; }
}
