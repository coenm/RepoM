namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

using System;
using Newtonsoft.Json.Linq;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class ActionGitFetchV1Deserializer : IActionDeserializer
{
    bool IActionDeserializer.CanDeserialize(string type)
    {
        return "git-fetch@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer)
    {
        return Deserialize(jToken);
    }

    private static RepositoryActionGitFetchV1? Deserialize(JToken jToken)
    {
        return jToken.ToObject<RepositoryActionGitFetchV1>();
    }
}