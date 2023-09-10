namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.Git.RepositoryAction;

public class ActionBrowserV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionBrowserV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }

    bool IActionToRepositoryActionMapper.CanMap(Data.RepositoryAction action)
    {
        return action is RepositoryActionBrowserV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(Data.RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionBrowserV1, repository);
    }

    private IEnumerable<RepositoryAction> Map(RepositoryActionBrowserV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        if (action.Url is null)
        {
            yield break;
        }

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        var url = _expressionEvaluator.EvaluateStringExpression(action.Url, repository);
        yield return new RepositoryAction(name, repository)
            {
                Action = new DelegateAction((_, _) => ProcessHelper.StartProcess(url, string.Empty)),
            };
    }
}