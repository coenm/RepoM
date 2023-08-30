namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.IO.Variables;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;
using RepositoryAction = RepoM.Api.Git.RepositoryAction;

public class ActionMapperComposition
{
    private readonly IActionToRepositoryActionMapper[] _deserializers;
    private readonly IRepositoryExpressionEvaluator _repoExpressionEvaluator;

    public ActionMapperComposition(IEnumerable<IActionToRepositoryActionMapper> deserializers, IRepositoryExpressionEvaluator repoExpressionEvaluator)
    {
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _deserializers = deserializers.ToArray();
    }

    public RepositoryActionBase[] Map(Data.RepositoryAction action, Repository repository)
    {
        if (repository == null)
        {
            throw new ArgumentNullException(nameof(repository));
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

        IActionToRepositoryActionMapper? deserializer = _deserializers.FirstOrDefault(x => x.CanMap(action));

        using IDisposable disposable = RepoMVariableProviderStore.Push(EvaluateVariables(action.Variables));

        IEnumerable<RepositoryActionBase> result = deserializer?.Map(action, repository, this) ?? Enumerable.Empty<RepositoryAction>();

        return result.ToArray();
    }

    private object? Evaluate(object? input, IRepository repository)
    {
        if (input is string s)
        {
            return _repoExpressionEvaluator.EvaluateValueExpression(s, repository);
        }

        return input;
    }

    private bool IsEnabled(string? booleanExpression, bool defaultWhenNullOrEmpty, IRepository repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _repoExpressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
    }
}