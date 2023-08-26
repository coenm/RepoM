namespace RepoM.Api.IO.ExpressionEvaluator;

using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.Parser;
using ExpressionStringEvaluator.VariableProviders;
using RepoM.Core.Plugin.Expressions;
using RepoM.Core.Plugin.Repository;

public class RepositoryExpressionEvaluator : IRepositoryExpressionEvaluator
{
    private readonly ExpressionExecutor _expressionExecutor;

    public RepositoryExpressionEvaluator(IEnumerable<IVariableProvider> variableProviders, IEnumerable<IMethod> methods)
    {
        List<IVariableProvider> v = variableProviders.ToList() ?? throw new ArgumentNullException(nameof(variableProviders));
        List<IMethod> m = methods.ToList() ?? throw new ArgumentNullException(nameof(methods));

        _expressionExecutor = new ExpressionExecutor(v, m);
    }

    public bool EvaluateBooleanExpression(string? value, IRepository? repository)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if ("true".Equals(value, StringComparison.InvariantCulture))
        {
            return true;
        }

        if ("false".Equals(value, StringComparison.InvariantCulture))
        {
            return false;
        }

        try
        {
            object? result = _expressionExecutor.Execute(new RepositoryContext(repository), value!);

            if (result is null)
            {
                return false;
            }

            if (result is bool b)
            {
                return b;
            }

            if (result is string s)
            {
                return "true".Equals(s, StringComparison.CurrentCultureIgnoreCase);
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public object? EvaluateValueExpression(string value, IRepository? repository)
    {
        try
        {
            return _expressionExecutor.Execute(new RepositoryContext(repository), value);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public string EvaluateStringExpression(string value, IRepository? repository)
    {
        try
        {
            object? result = _expressionExecutor.Execute(new RepositoryContext(repository), value);

            // seems to be possible
            if (result == null)
            {
                return string.Empty;
            }

            if (result is string s)
            {
                return s;
            }

            return string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}