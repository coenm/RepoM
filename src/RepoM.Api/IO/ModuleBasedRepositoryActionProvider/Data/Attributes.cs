namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

using System;

/// <summary>
/// Attribute indicating what the property type should be.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyTypeAttribute : Attribute
{
    public PropertyTypeAttribute(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
}

[AttributeUsage(AttributeTargets.Property)]
public abstract class PropertyDefaultValueAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyDefaultBoolValueAttribute : PropertyDefaultValueAttribute
{
    public PropertyDefaultBoolValueAttribute(bool defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public bool DefaultValue { get; }
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertyDefaultTypedValueAttribute<T> : PropertyDefaultValueAttribute
{
    public PropertyDefaultTypedValueAttribute(T defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public T DefaultValue { get; }
}