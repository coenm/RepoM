namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.RepositoryActions.Executors.Delegate;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionPinRepositoryV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryMonitor _repositoryMonitor;

    public ActionPinRepositoryV1Mapper(
        RepositoryExpressionEvaluator expressionEvaluator,
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

    public bool CanHandleMultipleRepositories()
    {
        return true;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        foreach (Repository r in repository)
        {
            RepositoryActionBase[] result = Map(action as RepositoryActionPinRepositoryV1, r).ToArray();
            if (result.Any())
            {
                return result;
            }
        }

        return Array.Empty<Git.RepositoryAction>();
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
                    _ => throw new ArgumentOutOfRangeException("Unexpected value for mode.")
                };
        }

        Git.RepositoryAction CreateAction(string name, Repository repository, bool newPinnedValue)
        {
            return new Git.RepositoryAction(name)
                {
                    Action = new DelegateAction((_, _) => _repositoryMonitor.SetPinned(newPinnedValue, repository)),
                };
        }

        yield return action.Mode switch
            {
                RepositoryActionPinRepositoryV1.PinMode.Toggle => CreateAction(name, repository, !currentlyPinned),
                RepositoryActionPinRepositoryV1.PinMode.Pin => CreateAction(name, repository, true),
                RepositoryActionPinRepositoryV1.PinMode.UnPin => CreateAction(name, repository, false),
                _ => throw new ArgumentOutOfRangeException(nameof(action.Mode), action.Mode, "Not expected")
            };
    }
}