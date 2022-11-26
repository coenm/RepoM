namespace RepoM.Plugin.SonarCloud;

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.RepositoryActions.Executors.Delegate;
using RepoM.Core.Plugin.RepositoryActions.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

[UsedImplicitly]
internal class ActionSonarCloudV1Mapper : IActionToRepositoryActionMapper
{
    private readonly SonarCloudFavoriteService _service;
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionSonarCloudV1Mapper(SonarCloudFavoriteService service, RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionSonarCloudSetFavoriteV1;
    }

    public bool CanHandleMultipleRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionSonarCloudSetFavoriteV1, repository.First());
    }

    private IEnumerable<RepoM.Api.Git.RepositoryAction> Map(RepositoryActionSonarCloudSetFavoriteV1? action, Repository repository)
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

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        var key = _expressionEvaluator.EvaluateStringExpression(action.Project!, repository);

        if (_service.IsInitialized)
        {
            yield return new Api.Git.RepositoryAction(name)
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
            yield return new Api.Git.RepositoryAction(name)
                {
                    CanExecute = false,
                };
        }
    }
}