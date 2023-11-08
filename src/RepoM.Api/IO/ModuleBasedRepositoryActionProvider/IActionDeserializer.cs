namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

[Obsolete("Old action menu")]
public interface IActionDeserializer
{
    bool CanDeserialize(string type);

    RepositoryAction? Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer);
}