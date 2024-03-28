namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization.BufferedDeserialization.TypeDiscriminators;

internal class KeyValueTypeDiscriminatorWithDefaultType<TInterface, TDefaultImpl> : ITypeDiscriminator
    where TInterface : class
    where TDefaultImpl : class, TInterface
{
    private readonly KeyValueTypeDiscriminator _discriminator;

    public KeyValueTypeDiscriminatorWithDefaultType(string key, IDictionary<string, Type> mapping)
    {
        _discriminator = new KeyValueTypeDiscriminator(typeof(TInterface), key, mapping);
        BaseType = typeof(TInterface);
    }

    public Type BaseType { get; }

    public bool TryDiscriminate(IParser buffer, out Type? suggestedType)
    {
        var result = _discriminator.TryDiscriminate(buffer, out suggestedType);

        if (!result)
        {
            suggestedType = typeof(TDefaultImpl);
        }
        
        return true;
    }
}