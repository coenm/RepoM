namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.IO.Variables;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionForEachV1Mapper : IActionToRepositoryActionMapper
{
    private readonly RepositoryExpressionEvaluator _expressionEvaluator;

    public ActionForEachV1Mapper(RepositoryExpressionEvaluator expressionEvaluator)
    {
        _expressionEvaluator = expressionEvaluator ?? throw new ArgumentNullException(nameof(expressionEvaluator));
    }
    
    bool IActionToRepositoryActionMapper.CanMap(RepositoryAction action)
    {
        return action is RepositoryActionForEachV1;
    }

    bool IActionToRepositoryActionMapper.CanHandleMultipleRepositories()
    {
        return false;
    }

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionForEachV1, repository.First(), actionMapperComposition);
    }

    private object? Evaluate(object? input, Repository repository)
    {
        if (input is string s)
        {
            return _expressionEvaluator.EvaluateValueExpression(s, repository);
        }

        return input;
    }

    private IEnumerable<RepoM.Api.Git.RepositoryActionBase> Map(RepositoryActionForEachV1? action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        if (action == null)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(action.Variable))
        {
            yield break;
        }

        if (!_expressionEvaluator.EvaluateBooleanExpression(action.Active, repository))
        {
            yield break;
        }

        var loop = Evaluate(action.Loop, repository);

        if (loop is not IList list)
        {
            yield break;
        }

        if (list.Count == 0)
        {
            yield break;
        }
        
        foreach (var item in list)
        {
            if (item is null)
            {
                break;
            }

            using IDisposable disposable = RepoMVariableProviderStore.Push(new List<EvaluatedVariable>(1)
                {
                    new()
                        {
                            Name = action.Variable,
                            Value = Evaluate(item, repository),
                        },
                });


            foreach (RepositoryActionBase? repoAction in action.Actions.SelectMany(repositoryAction => actionMapperComposition.Map(repositoryAction, repository)))
            {
                yield return repoAction;
            }
        }
    }
}