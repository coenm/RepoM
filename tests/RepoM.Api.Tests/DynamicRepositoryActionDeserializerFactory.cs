namespace RepoM.Api.Tests;

using System;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

[Obsolete("old menu")]
internal static class DynamicRepositoryActionDeserializerFactory
{
    public static YamlDynamicRepositoryActionDeserializer Create()
    {
        return new YamlDynamicRepositoryActionDeserializer(CreateActionDeserializerComposition());
    }

    public static YamlDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        var actionDeserializerComposition = new ActionDeserializerComposition(new[] { actionDeserializer, }, Array.Empty<IKeyTypeRegistration<RepositoryAction>>());
        return new YamlDynamicRepositoryActionDeserializer(actionDeserializerComposition);
    }

    private static ActionDeserializerComposition CreateActionDeserializerComposition()
    {
        return new ActionDeserializerComposition(
            new IActionDeserializer[0],
            Array.Empty<IKeyTypeRegistration<RepositoryAction>>());
    }
}