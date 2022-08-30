namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class ActionIgnoreRepositoriesV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "ignore-repositories@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionIgnoreRepositoriesV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionIgnoreRepositoriesV1>(jsonSerializer);
    }
}