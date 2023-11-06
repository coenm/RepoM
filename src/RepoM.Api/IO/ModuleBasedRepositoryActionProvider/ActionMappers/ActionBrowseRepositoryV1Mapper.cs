namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

public class ActionBrowseRepositoryV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionBrowseRepositoryV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }

    bool IActionToRepositoryActionMapper.CanMap(Data.RepositoryAction action)
    {
        return action is RepositoryActionBrowseRepositoryV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(Data.RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionBrowseRepositoryV1, repository);
    }

    private IEnumerable<RepositoryAction> Map(RepositoryActionBrowseRepositoryV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        RepositoryAction? result = CreateBrowseRemoteAction(repository, action);
        if (result != null)
        {
            yield return result;
        }
    }

    private RepositoryAction? CreateBrowseRemoteAction(IRepository repository, RepositoryActionBrowseRepositoryV1 action)
    {
        if (repository.Remotes.Count == 0)
        {
            return null;
        }

        var forceSingle = false;
        if (!string.IsNullOrWhiteSpace(action.FirstOnly))
        {
            forceSingle = _expressionEvaluator.EvaluateBooleanExpression(action.FirstOnly, repository);
        }

        var actionName = _translationService.Translate("Browse remote");

        if (repository.Remotes.Count == 1 || forceSingle)
        {
            return CreateProcessRunnerAction(actionName, repository.Remotes[0].Url, repository);
        }

        return new DeferredSubActionsRepositoryAction(actionName, repository)
            {
                DeferredSubActionsEnumerator = () => repository.Remotes
                   .Take(50)
                   .Select(remote => CreateProcessRunnerAction(remote.Name, remote.Url, repository))
                   .ToArray(),
            };
    }

    private static RepositoryAction CreateProcessRunnerAction(string name, string process, IRepository repository, string arguments = "")
    {
        return new RepositoryAction(name, repository)
            {
                Action = new DelegateRepositoryCommand((_, _) => ProcessHelper.StartProcess(process, arguments)),
            };
    }
}