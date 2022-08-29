namespace RepoM.Plugin.AzureDevOps;

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

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
        RepositoryActionAzureDevOpsPullRequestsV1? result = jToken.ToObject<RepositoryActionAzureDevOpsPullRequestsV1>(jsonSerializer);

        JToken? showWhenEmpty = jToken["show-when-empty"];
        if (showWhenEmpty != null && result != null)
        {
            if (showWhenEmpty.Type == JTokenType.Boolean)
            {
                var value = showWhenEmpty.Value<bool>();
                result.ShowWhenEmpty = value ? "true" : "false";
            }

            if (showWhenEmpty.Type == JTokenType.String)
            {
                var value = showWhenEmpty.Value<string>();
                result.ShowWhenEmpty = value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    value = value.Trim();
                    if (string.Equals("true", value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        result.ShowWhenEmpty = "true";
                    }

                    if (string.Equals("false", value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        result.ShowWhenEmpty = "false";
                    }
                }
            }
        }

        JToken? projectId = jToken["project-id"];
        if (projectId != null && result != null)
        {
            if (projectId.Type == JTokenType.String)
            {
                result.ProjectId = projectId.Value<string>();
            }
        }

        JToken? repositoryId = jToken["repository-id"];
        if (repositoryId != null && result != null)
        {
            if (repositoryId.Type == JTokenType.String)
            {
                result.RepoId = repositoryId.Value<string>();
            }
        }

        return result;
    }
}