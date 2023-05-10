namespace RepoM.Plugin.AzureDevOps.ActionProvider;

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Plugin.AzureDevOps.ActionProvider.Options;
using RepositoryAction = Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionAzureDevOpsCreatePullRequestsV1Deserializer : IActionDeserializer
{
    public bool CanDeserialize(string type)
    {
        return "azure-devops-create-prs@1".Equals(type, StringComparison.CurrentCultureIgnoreCase);
    }

    RepositoryAction? IActionDeserializer.Deserialize(JToken jToken, ActionDeserializerComposition actionDeserializer, JsonSerializer jsonSerializer)
    {
        return Deserialize(jToken, jsonSerializer);
    }

    private static RepositoryActionAzureDevOpsCreatePullRequestsV1? Deserialize(JToken jToken, JsonSerializer jsonSerializer)
    {
        return jToken.ToObject<RepositoryActionAzureDevOpsCreatePullRequestsV1>(jsonSerializer);
    }
}