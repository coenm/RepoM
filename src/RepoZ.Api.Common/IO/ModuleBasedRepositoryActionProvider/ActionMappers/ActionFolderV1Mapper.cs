namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;
using RepoZ.Api.Common.Common;
using RepoZ.Api.Common.IO.ExpressionEvaluator;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepositoryAction = RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionFolderV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionFolderV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }
    
    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionFolderV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipleRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionFolderV1, repository.First(), actionMapperComposition);
    }

    private IEnumerable<RepoM.Api.Git.RepositoryAction> Map(RepositoryActionFolderV1? action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var deferred = false;
        if (!string.IsNullOrWhiteSpace(action.IsDeferred))
        {
            deferred = _expressionEvaluator.EvaluateBooleanExpression(action.IsDeferred, repository);
        }

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);

        if (deferred)
        {
            yield return new RepoM.Api.Git.RepositoryAction(name)
                {
                    CanExecute = true,
                    DeferredSubActionsEnumerator = () =>
                        action.Items
                              .Where(x => _expressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
                              .SelectMany(x => actionMapperComposition.Map(x, repository))
                              .ToArray(),
                };
        }
        else
        {
            yield return new RepoM.Api.Git.RepositoryAction(name)
                {
                    CanExecute = true,
                    SubActions = action.Items
                                       .Where(x => _expressionEvaluator.EvaluateBooleanExpression(x.Active, repository))
                                       .SelectMany(x => actionMapperComposition.Map(x, repository))
                                       .ToArray(),
                };
        }
    }
}