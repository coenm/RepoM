namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionPinRepositoryV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryMonitor _repositoryMonitor;

    public ActionPinRepositoryV1Mapper(
        IRepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService,
        IRepositoryMonitor repositoryMonitor)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
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

        var name = action.Name ?? string.Empty;
        if (!string.IsNullOrEmpty(name))
        {
            name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        }

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

        Git.RepositoryAction CreateAction(string name, Repository repository, bool newPinnedValue)
        {
            return new Git.RepositoryAction(name, repository)
                {
                    Action = new DelegateAction((_, _) => _repositoryMonitor.SetPinned(newPinnedValue, repository)),
                };
        }

        yield return action.Mode switch
            {
                RepositoryActionPinRepositoryV1.PinMode.Toggle => CreateAction(name, repository, !currentlyPinned),
                RepositoryActionPinRepositoryV1.PinMode.Pin => CreateAction(name, repository, true),
                RepositoryActionPinRepositoryV1.PinMode.UnPin => CreateAction(name, repository, false),
                _ => throw new NotImplementedException(),
            };
    }
}