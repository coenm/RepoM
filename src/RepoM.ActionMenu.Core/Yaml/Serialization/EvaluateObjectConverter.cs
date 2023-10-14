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
        return typeof(EvaluateObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
    }

    public object ReadYaml(IParser parser, Type type)
    {
        var value = ((Scalar)parser.Current).Value;
        parser.MoveNext();

        EvaluateObject? obj;
        if (_factory.TryGetValue(type, out Func<object>? factoryMethod))
        {
            obj = (EvaluateObject)factoryMethod.Invoke();
        }
        else
        {
            obj = (EvaluateObject)Activator.CreateInstance(type)!;
        }

        obj!.Value = value;
        return obj;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var stringValue = (value as EvaluateObject)?.Value;
        emitter.Emit(string.IsNullOrEmpty(stringValue)
            ? new Scalar(AnchorName.Empty, TagName.Empty, string.Empty)
            : new Scalar(stringValue!));
    }
}