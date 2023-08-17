namespace RepoM.Plugin.Heidi.Tests.TestFramework;

using System;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;
using RepoM.Plugin.Heidi.ActionProvider;

internal static class DynamicRepositoryActionDeserializerFactory
{
    public static YamlDynamicRepositoryActionDeserializer Create()
    {
        return new YamlDynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new IActionDeserializer[]
                    {
                        new DefaultActionDeserializer<RepositoryActionHeidiDatabasesV1>(),
                    },
                Array.Empty<IKeyTypeRegistration<RepositoryAction>>()));
    }

    public static YamlDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new YamlDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new[] { actionDeserializer, }, Array.Empty<IKeyTypeRegistration<RepositoryAction>>()));
    }
}