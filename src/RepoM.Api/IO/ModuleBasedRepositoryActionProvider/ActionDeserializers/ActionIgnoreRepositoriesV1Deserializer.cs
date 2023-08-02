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
        var value = RepositoryActionIgnoreRepositoriesV1.TYPE.Equals(type, StringComparison.CurrentCultureIgnoreCase);
        if (value)
        {
            return true;
        }

        value = CanDeserializeIgnoreRepositories(type);
        if (value)
        {
            // log
        }

        return value;
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionIgnoreRepositoriesV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionIgnoreRepositoriesV1>(jsonSerializer);
    }

    [Obsolete("Old type name.")]
    private static bool CanDeserializeIgnoreRepositories(string type)
    {
        return "ignore-repositories@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }
}