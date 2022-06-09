namespace RepoZ.Api.Common.IO.ExpressionEvaluator;

using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.Parser;
using ExpressionStringEvaluator.VariableProviders;
using System.Collections.Generic;
using System;
using System.Linq;
using RepoZ.Api.Git;

public class RepositoryContext
{
    public RepositoryContext()
    {
        Repositories = Array.Empty<Repository>();
    }

    public RepositoryContext(params Repository[] repositories)
    {
        Repositories = repositories.ToArray();
    }

    public RepositoryContext(IEnumerable<Repository> repositories)
    {
        Repositories = repositories.ToArray();
    }

    public Repository[] Repositories { get; }
}

public class RepositoryExpressionEvaluator
{
    private readonly ExpressionExecutor _expressionExecutor;

    public RepositoryExpressionEvaluator(IEnumerable<IVariableProvider> variableProviders, IEnumerable<IMethod> methods)
    {
        List<IVariableProvider> v = variableProviders?.ToList() ?? throw new ArgumentNullException(nameof(variableProviders));
        List<IMethod> m = methods?.ToList() ?? throw new ArgumentNullException(nameof(methods));

        _expressionExecutor = new ExpressionStringEvaluator.Parser.ExpressionExecutor(v, m);
    }

    public string EvaluateStringExpression(string value, params Repository[] repository)
    {
        return EvaluateStringExpression(value, repository.AsEnumerable());
    }

    private string EvaluateStringExpression(string value, IEnumerable<Repository> repository)
    {
        try
        {
            CombinedTypeContainer result = _expressionExecutor.Execute<RepositoryContext>(new RepositoryContext(repository), value);

            // seems to be possible
            if (result == null)
            {
                return string.Empty;
            }

            if (result.IsString(out var s))
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

    public bool EvaluateBooleanExpression(string? value, Repository? repository)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        try
        {
            Repository[] repositories = (repository == null) ? Array.Empty<Repository>() : new Repository[1] { repository };

            CombinedTypeContainer result = _expressionExecutor.Execute<RepositoryContext>(new RepositoryContext(repositories), value!);
            if (result.IsBool(out var b))
            {
                return b.Value;
            }

            if ("true".Equals(result.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}