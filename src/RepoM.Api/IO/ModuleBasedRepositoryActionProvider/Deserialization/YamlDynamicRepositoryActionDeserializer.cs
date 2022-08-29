namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;

using System;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using YamlDotNet.Serialization;

public class YamlDynamicRepositoryActionDeserializer
{
    private readonly JsonDynamicRepositoryActionDeserializer _jsonDynamicRepositoryActionDeserializer;
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    public YamlDynamicRepositoryActionDeserializer(JsonDynamicRepositoryActionDeserializer jsonDynamicRepositoryActionDeserializer)
    {
        _jsonDynamicRepositoryActionDeserializer = jsonDynamicRepositoryActionDeserializer ?? throw new ArgumentNullException(nameof(jsonDynamicRepositoryActionDeserializer));
        _deserializer = new DeserializerBuilder().Build();
        _serializer = new SerializerBuilder().JsonCompatible().Build();
    }

    public RepositoryActionConfiguration Deserialize(string rawContent)
    {
        var yamlObject = _deserializer.Deserialize(rawContent, typeof(object));
        if (yamlObject == null)
        {
            // todo, log, throw?, ..
            return _jsonDynamicRepositoryActionDeserializer.Deserialize(rawContent);
        }

        var json = _serializer.Serialize(yamlObject);
        return _jsonDynamicRepositoryActionDeserializer.Deserialize(json);
 }
}