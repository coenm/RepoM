namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class ActionSeparatorV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "separator@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionSeparatorV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionSeparatorV1>(jsonSerializer);
    }
}