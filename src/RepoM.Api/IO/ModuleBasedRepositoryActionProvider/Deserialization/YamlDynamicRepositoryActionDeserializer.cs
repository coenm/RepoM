namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using YamlDotNet.Serialization;

public class YamlDynamicRepositoryActionDeserializer : IRepositoryActionDeserializer
{
    private readonly JsonDynamicRepositoryActionDeserializer _jsonDynamicRepositoryActionDeserializer;
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    public YamlDynamicRepositoryActionDeserializer(ActionDeserializerComposition deserializers)
    {
        _jsonDynamicRepositoryActionDeserializer = new JsonDynamicRepositoryActionDeserializer(deserializers);
        _deserializer = new DeserializerBuilder().Build();
        _serializer = new SerializerBuilder().JsonCompatible().Build();
    }

    public RepositoryActionConfiguration Deserialize(string content)
    {
        var yamlObject = _deserializer.Deserialize(content, typeof(object));
        if (yamlObject == null)
        {
            // maybe it is json, just give it a try
            return _jsonDynamicRepositoryActionDeserializer.Deserialize(content);
        }

        var json = _serializer.Serialize(yamlObject);
        return _jsonDynamicRepositoryActionDeserializer.Deserialize(json);
    }
}