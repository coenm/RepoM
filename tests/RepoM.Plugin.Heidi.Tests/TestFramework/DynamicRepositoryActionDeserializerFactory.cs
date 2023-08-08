namespace RepoM.Plugin.Heidi.Tests.TestFramework;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Plugin.Heidi.ActionProvider;

internal static class DynamicRepositoryActionDeserializerFactory
{
    public static JsonDynamicRepositoryActionDeserializer Create()
    {
        return new JsonDynamicRepositoryActionDeserializer(
            new ActionDeserializerComposition(
                new IActionDeserializer[]
                    {
                        new DefaultActionDeserializer<RepositoryActionHeidiDatabasesV1>(),
                    }));
    }

    public static JsonDynamicRepositoryActionDeserializer CreateWithDeserializer(IActionDeserializer actionDeserializer)
    {
        return new JsonDynamicRepositoryActionDeserializer(new ActionDeserializerComposition(new[] { actionDeserializer, }));
    }
}