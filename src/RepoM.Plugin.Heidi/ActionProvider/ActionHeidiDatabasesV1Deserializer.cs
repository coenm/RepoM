namespace RepoM.Plugin.Heidi.ActionProvider;

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionHeidiDatabasesV1Deserializer : IActionDeserializer
{
    public bool CanDeserialize(string type)
    {
        return "heidi-databases@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, actionDeserializer, jsonSerializer);
    }

    private static RepositoryActionHeidiDatabasesV1? Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionHeidiDatabasesV1>(jsonSerializer);
    }
}