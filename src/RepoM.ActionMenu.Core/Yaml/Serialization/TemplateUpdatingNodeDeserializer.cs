namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

// ReSharper disable once UnusedTypeParameter, Justification: intentionally
internal class TemplateUpdatingNodeDeserializer<T> : INodeDeserializer where T : class, INodeDeserializer
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

        var props = expectedType
            .GetProperties(true)
            .Where(x =>
                x is { CanWrite: true, CanRead: true, } &&
                typeof(EvaluateObjectBase).GetTypeInfo().IsAssignableFrom(x.PropertyType.GetTypeInfo())
            )
            .ToArray();

        var props2 = value.GetType().GetProperties(true).Where(x =>
                x is { CanWrite: true, CanRead: true, } &&
                typeof(EvaluateObjectBase).GetTypeInfo().IsAssignableFrom(x.PropertyType.GetTypeInfo())
            )
            .ToArray();

        foreach (var prop in props.Concat(props2))
        {
            var y = prop.GetMethod!.Invoke(value, null);
            if (y == null)
            {
                continue;
            }

            if (prop.PropertyType == typeof(Script))
            {
                // no default value.
            }

            if (prop.PropertyType == typeof(Variable))
            {
                var attribute = prop.GetCustomAttributesData().SingleOrDefault(a =>
                    a.AttributeType.FullName == typeof(VariableAttribute).FullName);

                if (attribute != null)
                {
                    IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

                    if (constructorArguments.Count == 0)
                    {
                        (y as Variable)!.DefaultValue = null; // not needed?~
                    }
                }
            }

            if (prop.PropertyType == typeof(Predicate))
            {
                var attribute = prop.GetCustomAttributesData().SingleOrDefault(a =>
                    a.AttributeType.FullName == typeof(PredicateAttribute).FullName);

                if (attribute != null)
                {
                    IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

                    if (constructorArguments.Count == 1)
                    {
                        var defaultValue = (bool)constructorArguments[0].Value;
                        (y as Predicate)!.DefaultValue = defaultValue;
                    }
                }
            }

            if (prop.PropertyType == typeof(Text))
            {
                var attribute = prop.GetCustomAttributesData().SingleOrDefault(a =>
                    a.AttributeType.FullName == typeof(TextAttribute).FullName);

                if (attribute != null)
                {
                    var constructorArguments = attribute.ConstructorArguments;

                    if (constructorArguments.Count == 1)
                    {
                        var defaultValue = (string)constructorArguments[0].Value;
                        (y as Text)!.DefaultValue = defaultValue;
                    }
                }
            }

            // if (Nullable.GetUnderlyingType(prop.PropertyType) == typeof(RenderString))
            if (prop.PropertyType == typeof(Text))
            {
                var attribute = prop.GetCustomAttributesData().SingleOrDefault(a =>
                    a.AttributeType.FullName == typeof(RenderToNullableStringAttribute).FullName);

                if (attribute != null)
                {
                    var constructorArguments = attribute.ConstructorArguments;

                    if (constructorArguments.Count == 1)
                    {
                        var defaultValue = (string?)constructorArguments[0].Value;

                        if (defaultValue is not null)
                        {
                            if (y == null)
                            {
                                var newValue = new Text
                                {
                                    Value = string.Empty,
                                };
                                y = newValue;
                                prop.SetMethod!.Invoke(value, new[] { newValue, });
                            }
                        }

                        if (y != null)
                        {
                            (y as Text)!.DefaultValue = defaultValue;
                        }
                    }
                }
            }

            if (y is ICreateTemplate createTemplate)
            {
                createTemplate.CreateTemplate(_templateParser);
            }
        }

        return true;
    }
}