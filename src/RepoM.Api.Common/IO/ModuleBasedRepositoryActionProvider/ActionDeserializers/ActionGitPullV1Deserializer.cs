namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class ActionGitPullV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "git-pull@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionGitPullV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionGitPullV1>(jsonSerializer);
    }
}