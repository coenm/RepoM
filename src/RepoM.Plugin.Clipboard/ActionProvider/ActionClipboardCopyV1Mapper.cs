namespace RepoM.Plugin.Clipboard.ActionProvider;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Core.Plugin.Expressions;
using RepoM.Plugin.Clipboard.RepositoryAction.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionClipboardCopyV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionClipboardCopyV1Mapper(
        IRepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
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
        
        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        var txt = string.Empty;
        if (!string.IsNullOrWhiteSpace(action.Text))
        {
            txt = _expressionEvaluator.EvaluateStringExpression(action.Text, repository);
        }

        yield return new Api.Git.RepositoryAction(name, repository)
            {
                Action = new CopyToClipboardAction(txt),
                ExecutionCausesSynchronizing = false,
            };
    }
}