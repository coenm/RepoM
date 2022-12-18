namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.IO.Variables;
using RepoM.Core.Plugin.Expressions;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public class ActionForEachV1Mapper : IActionToRepositoryActionMapper
{
    private readonly IRepositoryExpressionEvaluator _expressionEvaluator;

    public ActionForEachV1Mapper(IRepositoryExpressionEvaluator expressionEvaluator)
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

    private bool IsEnabled(string? booleanExpression, bool defaultWhenNullOrEmpty, Repository repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _expressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
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

        var enumerable = Evaluate(action.Enumerable, repository);

        if (enumerable is not IList list)
        {
            yield break;
        }

        if (list.Count == 0)
        {
            yield break;
        }

        List<EvaluatedVariable> EvaluateVariables(IEnumerable<Variable> vars)
        {
            return vars
                   .Where(v => IsEnabled(v.Enabled, true, repository))
                   .Select(v => new EvaluatedVariable
                       {
                           Name = v.Name,
                           Value = Evaluate(v.Value, repository),
                       })
                   .ToList();
        }

        bool ShouldSkip(string? booleanExpression)
        {
            return !string.IsNullOrWhiteSpace(booleanExpression) && _expressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
        }

        foreach (var item in list)
        {
            if (item is null)
            {
                continue;
            }

            using IDisposable disposableIterationItem = RepoMVariableProviderStore.Push(new List<EvaluatedVariable>(1)
                {
                    new()
                        {
                            Name = action.Variable,
                            Value = Evaluate(item, repository),
                        },
                });

            if (ShouldSkip(action.Skip))
            {
                continue;
            }

            using IDisposable disposableDefinedVariables = RepoMVariableProviderStore.Push(EvaluateVariables(action.Variables));

            foreach (RepositoryActionBase? repoAction in action.Actions.SelectMany(repositoryAction => actionMapperComposition.Map(repositoryAction, repository)))
            {
                yield return repoAction;
            }
        }
    }
}