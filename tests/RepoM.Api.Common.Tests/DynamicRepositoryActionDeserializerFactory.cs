namespace RepoM.Api.Common.Tests;

using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;

internal static class DynamicRepositoryActionDeserializerFactory
{
    public static JsonDynamicRepositoryActionDeserializer Create()
    {
        return new JsonDynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new IActionDeserializer[]
                    {
                        new ActionExecutableV1Deserializer(),
                        new ActionCommandV1Deserializer(),
                        new ActionBrowserV1Deserializer(),
                        new ActionFolderV1Deserializer(),
                        new ActionSeparatorV1Deserializer(),
                        new ActionGitCheckoutV1Deserializer(),
                        new ActionGitFetchV1Deserializer(),
                        new ActionGitPushV1Deserializer(),
                        new ActionGitPullV1Deserializer(),
                        new ActionBrowseRepositoryV1Deserializer(),
                        new ActionIgnoreRepositoriesV1Deserializer(),
                        new ActionAssociateFileV1Deserializer(),
                    }));
    }

    public static JsonDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new JsonDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new IActionDeserializer[] { actionDeserializer, }));
    }
}