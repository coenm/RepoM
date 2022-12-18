namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Core.Plugin.Expressions;

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
        return action is RepositoryActionIgnoreRepositoriesV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipleRepositories()
    {
        return true;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(Data.RepositoryAction action, IEnumerable<Repository> repositories, ActionMapperComposition actionMapperComposition)
    {
        Repository[] repos = repositories as Repository[] ?? repositories.ToArray();
        if (repos.Any(r => !_expressionEvaluator.EvaluateBooleanExpression(action.Active, r)))
        {
            yield break;
        }
        
        yield return MultipleRepositoryActionHelper.CreateActionForMultipleRepositories(
            _translationService.Translate("Ignore"),
            repos,
            repository => _repositoryMonitor.IgnoreByPath(repository.Path));
    }
}