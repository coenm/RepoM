namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common.IO.ExpressionEvaluator;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using RepoM.Api.Git;
using RepositoryAction = RepoM.Api.Git.RepositoryAction;

public class ActionMapperComposition
{
    private readonly IActionToRepositoryActionMapper[] _deserializers;
    private readonly RepositoryExpressionEvaluator _repoExpressionEvaluator;

    public ActionMapperComposition(IEnumerable<IActionToRepositoryActionMapper> deserializers, RepositoryExpressionEvaluator repoExpressionEvaluator)
    {
        _repoExpressionEvaluator = repoExpressionEvaluator ?? throw new ArgumentNullException(nameof(repoExpressionEvaluator));
        _deserializers = deserializers.Where(x => x != null).ToArray() ?? throw new ArgumentNullException(nameof(deserializers));
    }

    public IEnumerable<RepositoryActionBase> Map(Data.RepositoryAction action, params Repository[] repositories)
    {
        Repository? singleRepository = repositories.Length <= 1 ? repositories.SingleOrDefault() : null;
        
        List<Variable> EvaluateVariables(IEnumerable<Variable> vars)
        {
            if (singleRepository == null)
            {
                return new List<Variable>(0);
            }

            return vars
                   .Where(v => IsEnabled(v.Enabled, true, singleRepository))
                   .Select(v => new Variable()
                       {
                           Name = v.Name,
                           Enabled = "true",
                           Value = Evaluate(v.Value, singleRepository),
                       })
                   .ToList();
        }

        IActionToRepositoryActionMapper? deserializer = _deserializers.FirstOrDefault(x => x.CanMap(action));

        using IDisposable disposable = RepoMVariableProviderStore.Push(EvaluateVariables(action.Variables));

        IEnumerable<RepositoryActionBase> result = deserializer?.Map(action, repositories, this) ?? Enumerable.Empty<RepositoryAction>();

        return result;

    }

    private string? Evaluate(string? input, Repository repository)
    {
        if (input == null)
        {
            return null;
        }

        return _repoExpressionEvaluator.EvaluateStringExpression(input, repository);
    }

    private bool IsEnabled(string? booleanExpression, bool defaultWhenNullOrEmpty, Repository repository)
    {
        return string.IsNullOrWhiteSpace(booleanExpression)
            ? defaultWhenNullOrEmpty
            : _repoExpressionEvaluator.EvaluateBooleanExpression(booleanExpression, repository);
    }
}