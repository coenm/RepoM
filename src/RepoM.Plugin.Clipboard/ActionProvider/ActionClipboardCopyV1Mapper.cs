namespace RepoM.Plugin.Clipboard.ActionProvider;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.Clipboard.RepositoryAction.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionClipboardCopyV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;

    public ActionClipboardCopyV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
    }

    public bool CanMap(RepositoryAction action)
    {
        return action is RepositoryActionClipboardCopyV1;
    }

    public IEnumerable<RepositoryActionBase> Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionClipboardCopyV1, repository);
    }

    private IEnumerable<RepositoryActionBase> Map(RepositoryActionClipboardCopyV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }
        
        var name = _expressionEvaluator.EvaluateNullStringExpression(action.Name, repository);
        var txt = _expressionEvaluator.EvaluateNullStringExpression(action.Text, repository);

        yield return new Api.RepositoryActions.RepositoryAction(name, repository)
            {
                Action = new CopyToClipboardAction(txt),
                ExecutionCausesSynchronizing = false,
            };
    }
}