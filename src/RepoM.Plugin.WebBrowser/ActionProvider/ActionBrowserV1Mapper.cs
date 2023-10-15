namespace RepoM.Plugin.WebBrowser.ActionProvider;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.WebBrowser.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.RepositoryActions.RepositoryAction;

[UsedImplicitly]
internal class ActionBrowserV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;

    public ActionBrowserV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
    }

    bool IActionToRepositoryActionMapper.CanMap(Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction action)
    {
        return action is RepositoryActionBrowserV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionBrowserV1, repository);
    }

    private IEnumerable<RepositoryAction> Map(RepositoryActionBrowserV1? action, IRepository repository)
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

        var name = _expressionEvaluator.EvaluateNullStringExpression(action.Name, repository);
        var url = _expressionEvaluator.EvaluateStringExpression(action.Url, repository);
        var profile = _expressionEvaluator.EvaluateNullStringExpression(action.Profile, repository);

        yield return new RepositoryAction(name, repository)
            {
                Action = new BrowseRepositoryCommand(url, profile),
            };
    }
}