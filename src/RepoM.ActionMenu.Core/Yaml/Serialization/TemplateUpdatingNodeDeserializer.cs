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

        PropertyInfo[] props = expectedType
           .GetProperties(true)
           .Where(x =>
               x is { CanWrite: true, CanRead: true, }
               &&
               typeof(EvaluateObjectBase).GetTypeInfo().IsAssignableFrom(x.PropertyType.GetTypeInfo())
           )
           .ToArray();

        PropertyInfo[] props2 = value.GetType()
            .GetProperties(true)
            .Where(x =>
                 x is { CanWrite: true, CanRead: true, }
                 &&
                 typeof(EvaluateObjectBase).GetTypeInfo().IsAssignableFrom(x.PropertyType.GetTypeInfo())
             )
             .ToArray();

        foreach (PropertyInfo prop in props.Concat(props2))
        {
            if (prop.Name == "Name")
            {
                // todo coenm remove, debug
                int i = 0;
            }

            var currentValue = prop.GetMethod!.Invoke(value, null);
            if (currentValue == null)
            {
                var isNullable = Nullable.GetUnderlyingType(prop.PropertyType) != null;
                if (!isNullable)
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
            }

            currentValue = prop.GetMethod!.Invoke(value, null);

            if (currentValue == null)
            {
                continue;
            }

            if (prop.PropertyType == typeof(Script))
            {
                // no default value.
            }

            if (prop.PropertyType == typeof(Variable))
            {
                CustomAttributeData? attribute = prop.GetCustomAttributesData()
                    .SingleOrDefault(a => a.AttributeType.FullName == typeof(VariableAttribute).FullName);

                if (attribute != null)
                {
                    IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

                    if (constructorArguments.Count == 0)
                    {
                        (currentValue as Variable)!.DefaultValue = null; // not needed?~
                    }
                }
            }

            if (prop.PropertyType == typeof(Predicate))
            {
                CustomAttributeData? attribute = prop.GetCustomAttributesData()
                    .SingleOrDefault(a => a.AttributeType.FullName == typeof(PredicateAttribute).FullName);

                if (attribute != null)
                {
                    IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

                    if (constructorArguments.Count == 1)
                    {
                        var defaultValue = (bool)constructorArguments[0].Value;
                        (currentValue as Predicate)!.DefaultValue = defaultValue;
                    }
                }
            }

            if (prop.PropertyType == typeof(Text))
            {
                CustomAttributeData? attribute = prop.GetCustomAttributesData()
                    .SingleOrDefault(a => a.AttributeType.FullName == typeof(TextAttribute).FullName);

                if (attribute != null)
                {
                    IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

                    if (constructorArguments.Count == 1)
                    {
                        var defaultValue = (string)constructorArguments[0].Value;
                        (currentValue as Text)!.DefaultValue = defaultValue;
                    }
                }
            }

            if (prop.PropertyType == typeof(Text))
            {
                CustomAttributeData? attribute = prop.GetCustomAttributesData()
                    .SingleOrDefault(a => a.AttributeType.FullName == typeof(RenderToNullableStringAttribute).FullName);

                if (attribute != null)
                {
                    IList<CustomAttributeTypedArgument> constructorArguments = attribute.ConstructorArguments;

                    if (constructorArguments.Count == 1)
                    {
                        var defaultValue = (string?)constructorArguments[0].Value;

                        if (defaultValue is not null)
                        {
                            if (currentValue == null)
                            {
                                var newValue = new Text
                                {
                                    Value = string.Empty,
                                };
                                currentValue = newValue;
                                prop.SetMethod!.Invoke(value, [newValue,]);
                            }
                        }

                        if (currentValue != null)
                        {
                            (currentValue as Text)!.DefaultValue = defaultValue;
                        }
                    }
                }
            }

            if (currentValue is ICreateTemplate createTemplate)
            {
                createTemplate.CreateTemplate(_templateParser);
            }
        }

        return true;
    }
}