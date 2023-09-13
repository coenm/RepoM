namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

public class ActionDeserializerComposition
{
    private readonly IActionDeserializer[] _deserializers;
    private readonly IKeyTypeRegistration<RepositoryAction>[] _typeRegistrations;

    public ActionDeserializerComposition(IEnumerable<IActionDeserializer> deserializers, IEnumerable<IKeyTypeRegistration<RepositoryAction>> typeRegistrations)
    {
        _deserializers = deserializers?.ToArray() ?? throw new ArgumentNullException(nameof(deserializers));
        _typeRegistrations = typeRegistrations.ToArray();
    }

    public RepositoryAction? DeserializeSingleAction(string type, JToken jToken, JsonSerializer jsonSerializer)
    {
        return DeserializeWithCustomDeserializers(type, jToken, jsonSerializer)
               ??
               DeserializeWithDefaultDeserializers(type, jToken, jsonSerializer);
    }

    private RepositoryAction? DeserializeWithCustomDeserializers(string type, JToken jToken, JsonSerializer jsonSerializer)
    {
        IActionDeserializer? deserializer = Array.Find(_deserializers, item => item.CanDeserialize(type));
        return deserializer?.Deserialize(jToken, this, jsonSerializer);
    }

    private RepositoryAction? DeserializeWithDefaultDeserializers(string type, JToken jToken, JsonSerializer jsonSerializer)
    {
        IKeyTypeRegistration<RepositoryAction>? registration = Array.Find(_typeRegistrations, x => x.Tag.Equals(type, StringComparison.CurrentCultureIgnoreCase));

        if (registration != null)
        {
            return jToken.ToObject(registration.ConfigurationType, jsonSerializer) as RepositoryAction;
        }

        return null;
    }
}