namespace RepoM.Plugin.SonarCloud;

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionSonarCloudV1Mapper : IActionToRepositoryActionMapper
{
    private readonly ISonarCloudFavoriteService _service;
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;

    public ActionSonarCloudV1Mapper(ISonarCloudFavoriteService service, IRepositoryExpressionEvaluator expressionEvaluator)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionSonarCloudSetFavoriteV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionSonarCloudSetFavoriteV1, repository);
    }

    private IEnumerable<Api.RepositoryActions.RepositoryAction> Map(RepositoryActionSonarCloudSetFavoriteV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(action.Project))
        {
            yield break;
        }

        var name = _expressionEvaluator.EvaluateNullStringExpression(action.Name, repository);
        var key = _expressionEvaluator.EvaluateStringExpression(action.Project!, repository);

        if (_service.IsInitialized)
        {
            yield return new Api.RepositoryActions.RepositoryAction(name, repository)
                {
                    Action = new DelegateAction((_, _) =>
                        {
                            try
                            {
                                _ = _service.SetFavorite(key);
                            }
                            catch (Exception)
                            {
                                // ignore
                            }
                        }),
                    ExecutionCausesSynchronizing = false,
                };
        }
        else
        {
            yield return new Api.RepositoryActions.RepositoryAction(name, repository)
                {
                    CanExecute = false,
                };
        }
    }
}