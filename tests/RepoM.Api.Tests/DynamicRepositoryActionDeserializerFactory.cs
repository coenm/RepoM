namespace RepoM.Api.Tests;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;

internal static class DynamicRepositoryActionDeserializerFactory
{
    public static JsonDynamicRepositoryActionDeserializer Create()
    {
        return new JsonDynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new IActionDeserializer[]
                    {
                        new ActionExecutableV1Deserializer(),
                        new DefaultActionDeserializer<RepositoryActionCommandV1>(),
                        new DefaultActionDeserializer<RepositoryActionBrowserV1>(),
                        new ActionFolderV1Deserializer(),
                        new DefaultActionDeserializer<RepositoryActionSeparatorV1>(),
                        new DefaultActionDeserializer<RepositoryActionGitCheckoutV1>(),
                        new DefaultActionDeserializer<RepositoryActionGitFetchV1>(),
                        new DefaultActionDeserializer<RepositoryActionGitPushV1>(),
                        new DefaultActionDeserializer<RepositoryActionGitPullV1>(),
                        new DefaultActionDeserializer<RepositoryActionBrowseRepositoryV1>(),
                        new DefaultActionDeserializer<RepositoryActionIgnoreRepositoryV1>(),
                        new DefaultActionDeserializer<RepositoryActionAssociateFileV1>(),
                        new DefaultActionDeserializer<RepositoryActionPinRepositoryV1>(),
                        new ActionForEachV1Deserializer(),
                        new DefaultActionDeserializer<RepositoryActionJustTextV1>(),
                    }));
    }

    public static JsonDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new JsonDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new[] { actionDeserializer, }));
    }
}