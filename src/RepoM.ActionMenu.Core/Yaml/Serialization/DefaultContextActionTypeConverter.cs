namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

/// <summary>
/// YamlTypeConverter to deserialize
/// - name: value
///
/// to ContextActionSetVariableV1 with Name = name, and Value = value
/// </summary>
/// <remarks>The value is always of type string, also when it is not inteded.</remarks>
internal class DefaultContextActionTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(DefaultV1Type);
    }

    public object ReadYaml(IParser parser, Type type)
    {
        if (parser.Current is not MappingStart)
        {
            throw new InvalidOperationException("Expected MappingStart " + parser.Current?.Start);
        }

        var key = GetStringFromScalar(parser);
        var value = GetStringFromScalar(parser);

        if (!parser.MoveNext())
        {
            throw new InvalidOperationException("Expected move next " + parser.Current?.Start);
        }
        if (parser.Current is not MappingEnd)
        {
            throw new InvalidOperationException("Expected MappingEnd " + parser.Current?.Start);
        }

        return new ContextActionSetVariableV1
            {
                Name = key,
                Value = value,
            };
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        throw new NotImplementedException();
    }

    private static string GetStringFromScalar(IParser parser)
    {
        if (!parser.MoveNext())
        {
            throw new InvalidOperationException("Expected move next " + parser.Current?.Start);
        }

        if (parser.Current is not Scalar scalar)
        {
            throw new InvalidOperationException("Expected Scalar " + parser.Current?.Start);
        }

        return scalar.Value;
    }
}