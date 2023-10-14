namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using RepoM.ActionMenu.Core.Yaml.Model.Ctx.SetVariable;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

internal class DefaultContextActionTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(DefaultContextActionType);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        /*
        parser.Current.GetType().Name "MappingStart"
        parser.MoveNext();
        parser.Current.GetType().Name "Scalar"
        parser.MoveNext();
        parser.Current.GetType().Name "Scalar"
        parser.MoveNext();
        parser.Current.GetType().Name "MappingEnd"
        */

        // expect key value
        parser.MoveNext();
        var key = ((Scalar)parser.Current).Value;
        parser.MoveNext();
        var value = ((Scalar)parser.Current).Value;
        parser.MoveNext();

        return new SetVariableContextAction()
            {
                Name = key,
                Value = value,
            };
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        throw new NotImplementedException();
    }
}