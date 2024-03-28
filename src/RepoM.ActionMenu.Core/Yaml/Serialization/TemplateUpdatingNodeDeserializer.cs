namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

// ReSharper disable once UnusedTypeParameter, Justification: intentionally
#pragma warning disable S2326
internal class TemplateUpdatingNodeDeserializer<T> : INodeDeserializer where T : class, INodeDeserializer
#pragma warning restore S2326
{
    private readonly INodeDeserializer _nodeDeserializer;
    private readonly ITemplateParser _templateParser;

    public TemplateUpdatingNodeDeserializer(INodeDeserializer nodeDeserializer, ITemplateParser templateParser)
    {
        _nodeDeserializer = nodeDeserializer ?? throw new ArgumentNullException(nameof(nodeDeserializer));
        _templateParser = templateParser ?? throw new ArgumentNullException(nameof(templateParser));
    }

    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        if (!_nodeDeserializer.Deserialize(reader, expectedType, nestedObjectDeserializer, out value))
        {
            return false;
        }

        if (value == null)
        {
            return true;
        }

        PropertyInfo[] props = GetPropertyInfos(expectedType);
        PropertyInfo[] props2 = GetPropertyInfos(value.GetType());

        foreach (PropertyInfo prop in props.Concat(props2))
        {
            var currentValue = prop.GetMethod!.Invoke(value, null);
            if (currentValue == null)
            {
                var isNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null;
                if (!isNullable)
                {
                    InitializeCurrentValue(value, prop);
                }
            }

            currentValue = prop.GetMethod!.Invoke(value, null);

            if (currentValue == null)
            {
                continue;
            }

            currentValue = SetDefaultValue(value, prop, currentValue);

            if (currentValue is ICreateTemplate createTemplate)
            {
                createTemplate.CreateTemplate(_templateParser);
            }
        }

        return true;
    }

    private static object? SetDefaultValue(object value, PropertyInfo prop, object currentValue)
    {
        if (prop.PropertyType == typeof(Script))
        {
            // no default value.
        }

        if (prop.PropertyType == typeof(Variable))
        {
            SetDefaultVariableValue(prop, currentValue);
        }

        if (prop.PropertyType == typeof(Predicate))
        {
            SetDefaultPredicateValue(prop, currentValue);
        }

        if (prop.PropertyType == typeof(Text))
        {
            SetDefaultTextValue(prop, currentValue);
        }

        if (prop.PropertyType == typeof(Text))
        {
            currentValue = SetDefaultNullableTextValue(value, prop, currentValue);
        }

        return currentValue;
    }

    private static void SetDefaultVariableValue(PropertyInfo prop, object currentValue)
    {
        if (!TryGetCustomAttributeData<VariableAttribute>(prop, out CustomAttributeData? attribute))
        {
            return;
        }

        IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

        if (constructorArguments.Count != 0)
        {
            return;
        }

        (currentValue as Variable)!.DefaultValue = null; // not needed?
    }

    private static void SetDefaultPredicateValue(PropertyInfo prop, object currentValue)
    {
        if (!TryGetCustomAttributeData<PredicateAttribute>(prop, out CustomAttributeData? attribute))
        {
            return;
        }

        IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

        if (constructorArguments.Count != 1)
        {
            return;
        }

        var defaultValue = (bool)constructorArguments[0].Value!; // we know it is a boolean.
        (currentValue as Predicate)!.DefaultValue = defaultValue;
    }

    private static void SetDefaultTextValue(PropertyInfo prop, object currentValue)
    {
        if (!TryGetCustomAttributeData<TextAttribute>(prop, out CustomAttributeData? attribute))
        {
            return;
        }

        IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

        if (constructorArguments.Count != 1)
        {
            return;
        }

        var defaultValue = (string)constructorArguments[0].Value!; // we know it is a string.
        (currentValue as Text)!.DefaultValue = defaultValue;
    }

    private static object SetDefaultNullableTextValue(object value, PropertyInfo prop, object currentValue)
    {
        if (!TryGetCustomAttributeData<RenderToNullableStringAttribute>(prop, out CustomAttributeData? attribute))
        {
            return currentValue;
        }

        IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

        if (constructorArguments.Count != 1)
        {
            return currentValue;
        }

        var defaultValue = (string?)constructorArguments[0].Value;

        if (defaultValue is not null && currentValue == null)
        {
            var newValue = new Text
                {
                    Value = string.Empty,
                };
            currentValue = newValue;
            prop.SetMethod!.Invoke(value, [newValue,]);
        }

        if (currentValue != null)
        {
            (currentValue as Text)!.DefaultValue = defaultValue ?? string.Empty;
        }

        return currentValue!;
    }

    private static void InitializeCurrentValue(object value, PropertyInfo prop)
    {
        // create
        if (prop.PropertyType == typeof(Predicate))
        {
            prop.SetMethod!.Invoke(value, [new ScribanPredicate(),]);
        }

        if (prop.PropertyType == typeof(Text))
        {
            prop.SetMethod!.Invoke(value, [new ScribanText(),]);
        }

        if (prop.PropertyType == typeof(Script))
        {
            prop.SetMethod!.Invoke(value, [new ScribanScript(),]);
        }

        if (prop.PropertyType == typeof(Variable))
        {
            prop.SetMethod!.Invoke(value, [new ScribanText(),]);
        }
    }

    private static PropertyInfo[] GetPropertyInfos(Type type)
    {
        return type
               .GetProperties(true)
               .Where(propertyInfo =>
                   propertyInfo is { CanWrite: true, CanRead: true, }
                   &&
                   typeof(EvaluateObjectBase).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo())
               ).ToArray();
    }

    private static bool TryGetCustomAttributeData<TAttribute>(PropertyInfo propertyInfo, [NotNullWhen(true)] out CustomAttributeData? customAttributeData ) where TAttribute : Attribute
    {
        customAttributeData = propertyInfo.GetCustomAttributesData().SingleOrDefault(a => a.AttributeType.FullName == typeof(TAttribute).FullName);
        return customAttributeData != null;
    }
}