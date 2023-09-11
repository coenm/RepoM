namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

public class ActionIgnoreRepositoriesV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryMonitor _repositoryMonitor;

    public ActionIgnoreRepositoriesV1Mapper(
        IRepositoryExpressionEvaluator expressionEvaluator,
        ITranslationService translationService,
        IRepositoryMonitor repositoryMonitor)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryMonitor = repositoryMonitor ?? throw new ArgumentNullException(nameof(repositoryMonitor));
    }

    bool IActionToRepositoryActionMapper.CanMap(Data.RepositoryAction action)
    {
        return action is RepositoryActionIgnoreRepositoryV1;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(Data.RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        yield return new RepositoryAction(_translationService.Translate("Ignore"), repository)
            {
                Action = new DelegateAction((_, _) =>
                    {
                        try
                        {
                            _repositoryMonitor.IgnoreByPath(repository.Path);
                        }
                        catch
                        {
                            // nothing to see here
                        }
                    }),
                ExecutionCausesSynchronizing = true,
            };
    }
}