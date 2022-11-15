namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using System.IO.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

internal static class ActionMapperCompositionFactory
{
    public static ActionMapperComposition Create(
        RepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService,
        IFileSystem fileSystem,
        IRepositoryWriter repositoryWriter,
        IRepositoryMonitor repositoryMonitor)
    {
        var list = new List<IActionToRepositoryActionMapper>
            {
                new ActionBrowseRepositoryV1Mapper(expressionEvaluator, translationService),
                new ActionBrowserV1Mapper(expressionEvaluator, translationService),
                new ActionCommandV1Mapper(expressionEvaluator, translationService),
                new ActionExecutableV1Mapper(expressionEvaluator,translationService, fileSystem),
                new ActionFolderV1Mapper(expressionEvaluator, translationService),
                new ActionForEachV1Mapper(expressionEvaluator),
                new ActionGitCheckoutV1Mapper(expressionEvaluator, translationService, repositoryWriter),
                new ActionGitFetchV1Mapper(expressionEvaluator, translationService, repositoryWriter),
                new ActionGitPullV1Mapper(expressionEvaluator, translationService, repositoryWriter),
                new ActionGitPushV1Mapper(expressionEvaluator, translationService, repositoryWriter),
                new ActionIgnoreRepositoriesV1Mapper(expressionEvaluator, translationService, repositoryMonitor),
                new ActionSeparatorV1Mapper(expressionEvaluator),
                new ActionAssociateFileV1Mapper(expressionEvaluator, translationService),
            };

        return new ActionMapperComposition(list, expressionEvaluator);
    }
}