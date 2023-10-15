namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Commands;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
public class ActionPinRepositoryV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly IRepositoryMonitor _repositoryMonitor;

    public ActionPinRepositoryV1Mapper(
        IRepositoryExpressionEvaluator expressionEvaluator,
        IRepositoryMonitor repositoryMonitor)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _repositoryMonitor = repositoryMonitor ?? throw new ArgumentNullException(nameof(repositoryMonitor));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionPinRepositoryV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionPinRepositoryV1, repository);
    }

    private IEnumerable<RepositoryActionBase> Map(RepositoryActionPinRepositoryV1? action, Repository repository)
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

        var currentlyPinned = _repositoryMonitor.IsPinned(repository);

        const string PIN = "Pin Repository";
        const string UNPIN = "Unpin Repository";

        if (string.IsNullOrWhiteSpace(name))
        {
            name = action.Mode switch
            {
                RepositoryActionPinRepositoryV1.PinMode.Toggle => currentlyPinned ? UNPIN : PIN,
                RepositoryActionPinRepositoryV1.PinMode.Pin => PIN,
                RepositoryActionPinRepositoryV1.PinMode.UnPin => UNPIN,
                _ => throw new NotImplementedException(),
            };
        }

        yield return action.Mode switch
            {
                RepositoryActionPinRepositoryV1.PinMode.Toggle => CreateAction(name, repository, !currentlyPinned),
                RepositoryActionPinRepositoryV1.PinMode.Pin => CreateAction(name, repository, true),
                RepositoryActionPinRepositoryV1.PinMode.UnPin => CreateAction(name, repository, false),
                _ => throw new NotImplementedException(),
            };

        RepositoryActions.RepositoryAction CreateAction(string actionName, Repository repo, bool newPinnedValue)
        {
            return new RepositoryActions.RepositoryAction(actionName, repo)
                {
                    Action = new DelegateRepositoryCommand((_, _) => _repositoryMonitor.SetPinned(newPinnedValue, repo)),
                };
        }
    }
}