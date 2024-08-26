namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using System.Reflection;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

internal class EvaluateObjectConverter : IYamlTypeConverter
{
    private readonly Dictionary<Type, Func<object>> _factory;

    public EvaluateObjectConverter(Dictionary<Type, Func<object>> factory)
    {
        _factory = factory;
    }

    public bool Accepts(Type type)
    {
        return typeof(EvaluateObjectBase).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
    }

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        ParsingEvent current = parser.Current ?? throw new YamlException("No current event.");

        var value = ((Scalar)current).Value;

        if (!parser.MoveNext())
        {
            throw new YamlException("No next item found.");
        }

        EvaluateObjectBase obj;
        if (_factory.TryGetValue(type, out Func<object>? factoryMethod))
        {
            obj = (EvaluateObjectBase)factoryMethod.Invoke();
        }
        else
        {
            obj = (EvaluateObjectBase)Activator.CreateInstance(type)!;
        }

        obj.Value = value;
        return obj;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var stringValue = (value as EvaluateObjectBase)?.Value;
        emitter.Emit(string.IsNullOrEmpty(stringValue)
            ? new Scalar(AnchorName.Empty, TagName.Empty, string.Empty)
            : new Scalar(stringValue));
    }
}