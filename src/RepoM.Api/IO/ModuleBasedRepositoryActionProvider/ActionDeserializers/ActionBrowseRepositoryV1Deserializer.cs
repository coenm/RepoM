namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class ActionBrowseRepositoryV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "browse-repository@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionBrowseRepositoryV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionBrowseRepositoryV1>(jsonSerializer);
    }
}