namespace RepoM.Plugin.AzureDevOps;

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepositoryAction = RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionAzureDevOpsPullRequestsV1Deserializer : IActionDeserializer
{
    public bool CanDeserialize(string type)
    {
        return "azure-devops-get-prs@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionAzureDevOpsPullRequestsV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionAzureDevOpsPullRequestsV1>(jsonSerializer);
    }
}