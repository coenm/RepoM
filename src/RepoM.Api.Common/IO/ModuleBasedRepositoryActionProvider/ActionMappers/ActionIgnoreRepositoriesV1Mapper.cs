namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common.Common;
using RepoM.Api.Common.IO.ExpressionEvaluator;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.Git;

public class ActionIgnoreRepositoriesV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryMonitor _repositoryMonitor;

    public ActionIgnoreRepositoriesV1Mapper(
        RepositoryExpressionEvaluator expressionEvaluator,
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