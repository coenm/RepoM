namespace RepoM.Api.Tests;

using System;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionDeserializers;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Deserialization;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

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
                },
            Array.Empty<IKeyTypeRegistration<RepositoryAction>>());
    }
}