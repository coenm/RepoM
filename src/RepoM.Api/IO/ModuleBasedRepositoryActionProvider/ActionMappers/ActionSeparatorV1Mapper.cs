namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionSeparatorV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;

    public ActionSeparatorV1Mapper(RepositoryExpressionEvaluator expressionEvaluator)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
    }

    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionSeparatorV1;
    }

    public bool CanHandleMultipleRepositories()
    {
        return true;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        foreach (Repository r in repository)
        {
            RepositoryActionBase[] result = Map(action as RepositoryActionSeparatorV1, r).ToArray();
            if (result.Any())
            {
                return result;
            }
        }

        return Array.Empty<RepoM.Api.Git.RepositoryAction>();
    }

    private IEnumerable<RepositoryActionBase> Map(RepositoryActionSeparatorV1? action, Repository repository)
    {
        if (action == null)
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        yield return new RepositorySeparatorAction(repository);
    }
}