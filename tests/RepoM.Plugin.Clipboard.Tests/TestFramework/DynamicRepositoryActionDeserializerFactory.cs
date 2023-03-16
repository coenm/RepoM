namespace RepoM.Plugin.Clipboard.Tests.TestFramework;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Plugin.Clipboard.ActionProvider;

internal static class DynamicRepositoryActionDeserializerFactory
{
    public static JsonDynamicRepositoryActionDeserializer Create()
    {
        return new JsonDynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new IActionDeserializer[]
                    {
                        new ActionClipboardCopyV1Deserializer(),
                    }));
    }

    public static JsonDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new JsonDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new[] { actionDeserializer, }));
    }
}