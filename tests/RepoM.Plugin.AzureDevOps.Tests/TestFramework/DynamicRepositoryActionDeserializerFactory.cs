namespace RepoM.Plugin.AzureDevOps.Tests.TestFramework;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Plugin.AzureDevOps.ActionProvider.Options;

internal static class DynamicRepositoryActionDeserializerFactory
{
    public static JsonDynamicRepositoryActionDeserializer Create()
    {
        return new JsonDynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new IActionDeserializer[]
                    {
                        new DefaultActionDeserializer<RepositoryActionAzureDevOpsCreatePullRequestsV1>(),
                        new DefaultActionDeserializer<RepositoryActionAzureDevOpsGetPullRequestsV1>(),
                    }));
    }

    public static JsonDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new JsonDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new[] { actionDeserializer, }));
    }
}