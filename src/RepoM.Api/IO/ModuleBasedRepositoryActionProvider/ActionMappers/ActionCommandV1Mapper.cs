namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionCommandV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;
    private readonly ITranslationService _translationService;

    public ActionCommandV1Mapper(RepositoryExpressionEvaluator expressionEvaluator, ITranslationService translationService)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
        _translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionCommandV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipleRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionCommandV1, repository.First());
    }

    private IEnumerable<RepoM.Api.Git.RepositoryAction> Map(RepositoryActionCommandV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var name = NameHelper.EvaluateName(action.Name, repository, _translationService, _expressionEvaluator);
        var command = _expressionEvaluator.EvaluateStringExpression(action.Command ?? string.Empty, repository);
        var arguments = _expressionEvaluator.EvaluateStringExpression(action.Arguments ?? string.Empty, repository); 

        yield return new RepoM.Api.Git.RepositoryAction(name)
            {
                Action = (_, _) => ProcessHelper.StartProcess(command, arguments),
            };
    }
}