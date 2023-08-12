namespace RepoM.Plugin.Clipboard.Tests.TestFramework;

using System;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Plugin.Clipboard.ActionProvider;

internal static class DynamicRepositoryActionDeserializerFactory
{
    public static JsonDynamicRepositoryActionDeserializer Create()
    {
        return new JsonDynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new IActionDeserializer[]
                    {
                        new DefaultActionDeserializer<RepositoryActionClipboardCopyV1>(),
                    },
                Array.Empty<IKeyTypeRegistration<RepositoryAction>>()));
    }

    public static JsonDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new JsonDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new[] { actionDeserializer, }, Array.Empty<IKeyTypeRegistration<RepositoryAction>>()));
    }
}