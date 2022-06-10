namespace RepoZ.Api.Common.Tests.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using System.IO.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

internal static class ActionMapperCompositionFactory
{
    public static ActionMapperComposition Create(
        RepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService,
        IErrorHandler errorHandler,
        IFileSystem fileSystem,
        IRepositoryWriter repositoryWriter,
        IRepositoryMonitor repositoryMonitor)
    {
        var list = new List<IActionToRepositoryActionMapper>
            {
                new ActionBrowseRepositoryV1Mapper(expressionEvaluator, translationService, errorHandler),
                new ActionBrowserV1Mapper(expressionEvaluator, translationService, errorHandler),
                new ActionCommandV1Mapper(expressionEvaluator, translationService, errorHandler),
                new ActionExecutableV1Mapper(expressionEvaluator,translationService, errorHandler, fileSystem),
                new ActionFolderV1Mapper(expressionEvaluator, translationService),
                new ActionGitCheckoutV1Mapper(expressionEvaluator, translationService, repositoryWriter),
                new ActionGitFetchV1Mapper(expressionEvaluator, translationService, repositoryWriter),
                new ActionGitPullV1Mapper(expressionEvaluator, translationService, repositoryWriter),
                new ActionGitPushV1Mapper(expressionEvaluator, translationService, repositoryWriter),
                new ActionIgnoreRepositoriesV1Mapper(expressionEvaluator, translationService, repositoryMonitor),
                new ActionSeparatorV1Mapper(expressionEvaluator),
                new ActionAssociateFileV1Mapper(expressionEvaluator, translationService, errorHandler),
            };

        return new ActionMapperComposition(list, expressionEvaluator);
    }
}