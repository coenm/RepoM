namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionGitPullV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;
    private readonly IRepositoryWriter _repositoryWriter;

    public ActionGitPullV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService, IRepositoryWriter repositoryWriter)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionGitPullV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipleRepositories()
    {
        return true;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repositories, ActionMapperComposition actionMapperComposition)
    {
        Repository[] repos = repositories as Repository[] ?? repositories.ToArray();
        if (repos.Any(r => !_expressionEvaluator.EvaluateBooleanExpression(action.Active, r)))
        {
            yield break;
        }

        yield return MultipleRepositoryActionHelper.CreateActionForMultipleRepositories(
            _translationService.Translate("Pull"),
            repos,
            _repositoryWriter.Pull,
            executionCausesSynchronizing: true);
    }
}