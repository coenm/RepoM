namespace RepoM.Api.Common.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ExpressionStringEvaluator.VariableProviders;
using RepoM.Api.Common.IO.ExpressionEvaluator;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;
using Repository = RepoM.Api.Git.Repository;

public static class EnvironmentVariableStore
{
    private static readonly AsyncLocal<Dictionary<string, string>> _envVars = new();

    public static IDisposable Set(Dictionary<string, string>? envVars)
    {
        if (envVars == null)
        {
            return new ExecuteOnDisposed(null );
        }

        _envVars.Value = envVars;
        return new ExecuteOnDisposed(() => _envVars.Value = new Dictionary<string, string>());
    }

    public static Dictionary<string, string> Get(Repository _)
    {
        return _envVars.Value ?? new Dictionary<string, string>(0);
    }
}

public class ExecuteOnDisposed : IDisposable
{
    private readonly Func<Dictionary<string, string>>? _func;

    public ExecuteOnDisposed(Func<Dictionary<string, string>>? func)
    {
        _func = func;
    }

    public void Dispose()
    {
        _func?.Invoke();
    }
}

public static class RepoMVariableProviderStore
{
    public static readonly AsyncLocal<Scope?> VariableScope = new();

    public static IDisposable Push(List<Variable> vars)
    {
        VariableScope.Value = new Scope(VariableScope.Value, vars);
        return VariableScope.Value;
    }
}

public class Scope : IDisposable
{
    // private readonly LoggerFactoryScopeProvider _provider;
    private bool _isDisposed;

    private Scope()
    {
        Parent = null;
        Variables = new List<Variable>(0);
    }

    public Scope(Scope? parent, List<Variable> variables)
    {
        // _provider = provider;
        Parent = parent;
        Variables = variables;
    }

    public static Scope Empty { get; } = new Scope();

    public Scope? Parent { get; }

    public List<Variable> Variables { get; }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            RepoMVariableProviderStore.VariableScope.Value = Parent;
            _isDisposed = true;
        }
    }
}

public class RepoMVariableProvider : IVariableProvider
{
    private const string PREFIX = "var.";

    /// <inheritdoc cref="IVariableProvider.CanProvide"/>
    public bool CanProvide(string key)
    {
        if (!key.StartsWith(PREFIX, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        var prefixLength = PREFIX.Length;
        if (key.Length <= prefixLength)
        {
            return false;
        }

        var envKey = key.Substring(prefixLength, key.Length - prefixLength);

        return !string.IsNullOrWhiteSpace(envKey);
    }

    /// <inheritdoc cref="IVariableProvider.Provide"/>
    public string Provide(string key, string? arg)
    {
        var prefixLength = PREFIX.Length;
        var envKey = key.Substring(prefixLength, key.Length - prefixLength);

        Scope? scope = RepoMVariableProviderStore.VariableScope.Value;

        while (true)
        {
            if (scope == null)
            {
                return string.Empty;
            }

            if (TryGetValueFromScope(scope, envKey, out var result))
            {
                return result;
            }

            scope = scope.Parent;
        }
    }

    private static bool TryGetValueFromScope(in Scope scope, string key, out string value)
    {
        Variable? var = scope.Variables.FirstOrDefault(x => key.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));

        if (var != null)
        {
            if ("false".Equals(var.Enabled, StringComparison.CurrentCultureIgnoreCase))
            {
                value = string.Empty;
                return true;
            }

            if (var.Value == null)
            {
                value = string.Empty;
                return false;
            }

            value = var.Value;
            return true;
        }

        value = string.Empty;
        return false;
    }
}

public class CustomEnvironmentVariableVariableProvider : IVariableProvider<RepositoryContext>
{
    private const string PREFIX = "Env.";

    /// <inheritdoc cref="IVariableProvider.CanProvide"/>
    public bool CanProvide(string key)
    {
        if (!key.StartsWith(PREFIX, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        var prefixLength = PREFIX.Length;
        if (key.Length <= prefixLength)
        {
            return false;
        }

        var envKey = key.Substring(prefixLength, key.Length - prefixLength);

        return !string.IsNullOrWhiteSpace(envKey);
    }

    public string Provide(RepositoryContext context, string key, string? arg)
    {
        var prefixLength = PREFIX.Length;
        var envKey = key.Substring(prefixLength, key.Length - prefixLength);

        Repository? singleContext = context.Repositories.SingleOrDefault();

        if (singleContext == null)
        {
            return Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
        }

        Dictionary<string, string> envVars = GetRepoEnvironmentVariables(singleContext);

        if (envVars.ContainsKey(envKey))
        {
            return envVars[envKey];
        }

        return Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
    }

    /// <inheritdoc cref="IVariableProvider.Provide"/>
    public string Provide(string key, string? arg)
    {
        var prefixLength = PREFIX.Length;
        var envKey = key.Substring(prefixLength, key.Length - prefixLength);
        var result = Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
        return result;
    }

    private static Dictionary<string, string> GetRepoEnvironmentVariables(Repository repository)
    {
        return EnvironmentVariableStore.Get(repository);
    }
}