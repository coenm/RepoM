namespace RepoM.Api.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using System.IO.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;

internal static class ActionMapperCompositionFactory
{
    public static ActionMapperComposition Create(
        IRepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService,
        IFileSystem fileSystem,
        IRepositoryWriter repositoryWriter,
        IRepositoryMonitor repositoryMonitor)
    {
        var mappers = new IActionToRepositoryActionMapper[]
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
                new ActionJustTextV1Mapper(expressionEvaluator, translationService),
            };

        return new ActionMapperComposition(mappers, expressionEvaluator);
    }

    public static ActionMapperComposition CreateSmall(IRepositoryExpressionEvaluator expressionEvaluator, IActionToRepositoryActionMapper actionToRepositoryActionMapper)
    {
        var list = new IActionToRepositoryActionMapper[1]
            {
                actionToRepositoryActionMapper
            };
        return new ActionMapperComposition(list, expressionEvaluator);
    }
}