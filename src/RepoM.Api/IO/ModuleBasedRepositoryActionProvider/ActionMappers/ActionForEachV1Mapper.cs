namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;
using RepoM.Api.RepositoryActions;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
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

    IEnumerable<RepositoryActionBase> IActionToRepositoryActionMapper.Map(RepositoryAction action, Repository repository, ActionMapperComposition actionMapperComposition)
    {
        return Map(action as RepositoryActionForEachV1, repository, actionMapperComposition);
    }

    private object? Evaluate(object? input, IRepository repository)
    {
        if (input is string s)
        {
            return _expressionEvaluator.EvaluateValueExpression(s, repository);
        }

        return input;
    }

    private bool IsEnabled(string? booleanExpression, bool defaultWhenNullOrEmpty, IRepository repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _expressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
    }

    private IEnumerable<RepositoryActionBase> Map(RepositoryActionForEachV1? action, Repository repository, ActionMapperComposition actionMapperComposition)
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

            if (ShouldSkip(action.Skip))
            {
                continue;
            }

            foreach (RepositoryActionBase? repoAction in action.Actions.SelectMany(repositoryAction => actionMapperComposition.Map(repositoryAction, repository)))
            {
                yield return repoAction;
            }
        }
    }
}