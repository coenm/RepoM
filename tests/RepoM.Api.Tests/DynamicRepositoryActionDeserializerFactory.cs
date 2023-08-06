namespace RepoM.Api.Tests;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
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
                        new ActionCommandV1Deserializer(),
                        new ActionBrowserV1Deserializer(),
                        new ActionFolderV1Deserializer(),
                        new ActionSeparatorV1Deserializer(),
                        new ActionGitCheckoutV1Deserializer(),
                        new ActionGitFetchV1Deserializer(),
                        new ActionGitPushV1Deserializer(),
                        new ActionGitPullV1Deserializer(),
                        new ActionBrowseRepositoryV1Deserializer(),
                        new ActionIgnoreRepositoryV1Deserializer(),
                        new ActionAssociateFileV1Deserializer(),
                        new ActionPinRepositoryV1Deserializer(),
                        new ActionForEachV1Deserializer(),
                        new ActionJustTextV1Deserializer(),
                    }));
    }

    public static JsonDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new JsonDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new[] { actionDeserializer, }));
    }
}