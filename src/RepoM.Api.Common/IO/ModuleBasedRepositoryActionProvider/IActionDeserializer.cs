namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using Newtonsoft.Json.Linq;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

public interface IActionDeserializer
{
    bool CanDeserialize(string type);

    RepositoryAction? Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer);
}