namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public interface IActionDeserializer
{
    bool CanDeserialize(string type);

    RepositoryAction? Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer);
}