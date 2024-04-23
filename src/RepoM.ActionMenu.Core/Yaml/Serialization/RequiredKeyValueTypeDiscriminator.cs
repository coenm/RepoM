namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.BufferedDeserialization;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

internal class RequiredKeyValueTypeDiscriminator<TInterface> : ITypeDiscriminator
    where TInterface : class
{
    private readonly KeyValueTypeDiscriminator _discriminator;
    private readonly ILogger _logger;

    public RequiredKeyValueTypeDiscriminator(string key, IDictionary<string, Type> mapping, ILogger logger)
    {
        _discriminator = new KeyValueTypeDiscriminator(typeof(TInterface), key, mapping);
        BaseType = typeof(TInterface);
        _logger = logger;
    }

    public Type BaseType { get; }

    public bool TryDiscriminate(IParser buffer, out Type? suggestedType)
    {
        var result = _discriminator.TryDiscriminate(buffer, out suggestedType);

        if (!result)
        {
            if (buffer.Current is Scalar scalar)
            {
                _logger.LogError("Could not find required type. Type found {Type}", scalar.Value);
            }
            else
            {
                _logger.LogError("Could not find required type");
            }

            suggestedType = null;
            return false;
        }

        return true;
    }
}