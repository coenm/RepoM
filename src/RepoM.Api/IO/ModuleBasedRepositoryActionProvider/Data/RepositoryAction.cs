namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using SimpleInjector;

// todo: move to plugin, make abstract
/// <summary>
/// Base for each type of Repository Actions.
/// </summary>
public /*abstract*/ class RepositoryAction
{
    /// <summary>
    /// RepositoryAction type. Should be a fixed value used to determine the action type.
    /// </summary>
    [Required]
    [PropertyType(typeof(string))]
    public string? Type { get; set; }

    /// <summary>
    /// Name of the action. This is shown in the UI of RepoM.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Name { get; set; }

    /// <summary>
    /// Is the action active (ie. visible) or not.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(true)]
    public string? Active { get; set; }

    /// <summary>
    /// Multiselect enabled.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(bool))] // todo
    [PropertyDefaultBoolValue(false)]
    public string? MultiSelectEnabled { get; set; }

    /// <summary>
    /// A set of variables to be availabe within this action.
    /// </summary>
    // [EvaluatedProperty]
    [PropertyType(typeof(List<Variable>))]
    public List<Variable> Variables { get; set; } = new List<Variable>();
}

/// <summary>
/// Attribute indicating that this property will be evaluated. Used for documation.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EvaluatedPropertyAttribute : Attribute
{
}

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
public sealed class PropertyDefaultStringValueAttribute : PropertyDefaultValueAttribute
{
    public PropertyDefaultStringValueAttribute(string defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public string DefaultValue { get; }
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
    public static void RegisterDefaultRepositoryActionDeserializerForType<T>(this Container container) where T : RepositoryAction
    {
        container.Collection.AppendInstance<IActionDeserializer>(new DefaultActionDeserializer<T>());
        // container.Collection.AppendInstance<IKeyTypeRegistration<RepositoryAction>>(new RepositoryActionKeyTypeRegistration<T>());
    }

    public static void RegisterDefaultRepositoryActionDeserializerForType(this Container container, Type type)
    {
        container.Collection.AppendInstance<IActionDeserializer>(new DefaultActionDeserializer(type));
        // container.Collection.AppendInstance<IKeyTypeRegistration<RepositoryAction>>(new RepositoryActionKeyTypeRegistration<T>());
    }
}


public class DefaultActionDeserializer : IActionDeserializer
{
    public DefaultActionDeserializer(Type t)
    {
        ConfigurationType = t;
        Tag = t.GetCustomAttribute<RepositoryActionAttribute>()?.Type ?? throw new InvalidOperationException($"RepositoryActionAttribute not found on {t.FullName}");
    }

    private Type ConfigurationType { get; }

    private string Tag { get; }


    public bool CanDeserialize(string type)
    {
        return Tag.Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    public RepositoryAction? Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject(ConfigurationType, jsonSerializer) as RepositoryAction;
    }
}


public class DefaultActionDeserializer<T> : IActionDeserializer where T : RepositoryAction
{
    public DefaultActionDeserializer()
    {
        ConfigurationType = typeof(T);
        Tag = typeof(T).GetCustomAttribute<RepositoryActionAttribute>()?.Type ?? throw new InvalidOperationException($"RepositoryActionAttribute not found on {typeof(T).FullName}");
    }

    private Type ConfigurationType { get; }

    private string Tag { get; }


    public bool CanDeserialize(string type)
    {
        return Tag.Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    public RepositoryAction? Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject(ConfigurationType, jsonSerializer) as RepositoryAction;
    }
}

public class RepositoryActionKeyTypeRegistration<T> : IKeyTypeRegistration<RepositoryAction> where T : RepositoryAction
{
    public RepositoryActionKeyTypeRegistration()
    {
        ConfigurationType = typeof(T);
        Tag = typeof(T).GetCustomAttribute<RepositoryActionAttribute>()?.Type ?? throw new InvalidOperationException($"RepositoryActionAttribute not found on {typeof(T).FullName}");
    }

    public Type ConfigurationType { get; }

    public string Tag { get; }
}
