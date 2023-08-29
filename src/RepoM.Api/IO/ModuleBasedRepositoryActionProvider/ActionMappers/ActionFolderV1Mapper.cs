namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionFolderV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionFolderV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }
    
    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionFolderV1;
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
            yield return new RepoM.Api.Git.RepositoryAction(name, repository)
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
            yield return new RepoM.Api.Git.RepositoryAction(name, repository)
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