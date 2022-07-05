namespace RepoM.Plugin.SonarCloud;

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.Git;
using RepositoryAction = RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionSonarCloudSetFavoriteV1Deserializer : IActionDeserializer
{
    public bool CanDeserialize(string type)
    {
        return "sonarcloud-set-favorite@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionSonarCloudSetFavoriteV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionSonarCloudSetFavoriteV1>(jsonSerializer);
    }
}