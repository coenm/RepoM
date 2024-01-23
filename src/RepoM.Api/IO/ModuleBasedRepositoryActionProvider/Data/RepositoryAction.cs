namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

using System;
using System.Reflection;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using SimpleInjector;

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

public static class ContainerExtensions
{
    // todo move to plugin interface
    public static void RegisterActionMenuMapper<T>(this Container container, Lifestyle lifestyle) where T : class, RepoM.ActionMenu.Interface.YamlModel.IActionToRepositoryActionMapper
    {
        container.Collection.Append<RepoM.ActionMenu.Interface.YamlModel.IActionToRepositoryActionMapper, T>(lifestyle);
    }

    // todo move to plugin interface
    public static void RegisterActionMenuType<T>(this Container container) where T : IMenuAction
    {
        RegisterActionMenuType(container, typeof(T));
    }

    // todo move to plugin interface
    public static void RegisterActionMenuType(this Container container, Type type)
    {
        container.Collection.AppendInstance<IKeyTypeRegistration<IMenuAction>>(new FixedTypeRegistration<IMenuAction>(type, TypeRepositoryActionAttributeReader.GetValue(type)));
    }

    // stupid, for test purposes. todo
    public static IKeyTypeRegistration<IMenuAction> CreateRegistrationObject<T>()
    {
        var type = typeof(T);
        return new FixedTypeRegistration<IMenuAction>(type, TypeRepositoryActionAttributeReader.GetValue(type));
    }
}

/// <summary>
/// This class 'assumes' that the type has a custom attribute of type <see cref="RepositoryActionAttribute"/> with a property 'Type' that is the type value.
/// </summary>
file static class TypeRepositoryActionAttributeReader
{
    public static string GetValue(Type type)
    {
        return type.GetCustomAttribute<RepositoryActionAttribute>()?.Type ?? throw new InvalidOperationException($"RepositoryActionAttribute not found on {type.FullName}");
    }
}
