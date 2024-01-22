namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[Obsolete("Old action menu")]
public class ActionFolderV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;

    public ActionFolderV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
    }
    
    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionFolderV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionFolderV1, repository, actionMapperComposition);
    }

    private IEnumerable<RepositoryActions.RepositoryAction> Map(RepositoryActionFolderV1? action, Repository repository, ActionMapperComposition actionMapperComposition)
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

        var name = _expressionEvaluator.EvaluateNullStringExpression(action.Name, repository);

        if (deferred)
        {
            yield return new DeferredSubActionsRepositoryAction(name, repository)
                {
                    CanExecute = true,
                    DeferredSubActionsEnumerator = () =>
                        action.Items
                          .Where(repoAction => _expressionEvaluator.EvaluateBooleanExpression(repoAction.Active, repository))
                          .SelectMany(repoAction => actionMapperComposition.Map(repoAction, repository))
                          .ToArray(),
                };
        }
        else
        {
            yield return new RepositoryActions.RepositoryAction(name, repository)
                {
                    CanExecute = true,
                    SubActions = action.Items
                       .Where(repoAction => _expressionEvaluator.EvaluateBooleanExpression(repoAction.Active, repository))
                       .SelectMany(repoAction => actionMapperComposition.Map(repoAction, repository))
                       .ToArray(),
                };
        }
    }
}