namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Collections.Generic;
using ExpressionStringEvaluator.VariableProviders;
using JetBrains.Annotations;
using RepoM.Api.IO.Variables;
using RepoM.Core.Plugin.Repository;

[UsedImplicitly]
public class CustomEnvironmentVariableVariableProvider : RepoM.Core.Plugin.VariableProviders.IVariableProvider<RepositoryContext>
{
    private const string PREFIX = "Env.";

    /// <inheritdoc cref="IVariableProvider.CanProvide"/>
    public bool CanProvide(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (key.Length <= PREFIX.Length)
        {
            return false;
        }

        if (!key.StartsWith(PREFIX, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        var envKey = key[PREFIX.Length..];
        return !string.IsNullOrWhiteSpace(envKey);
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        IRepository? repository = context?.Repository;
        if (repository == null)
        {
            return Provide(key, arg);
        }

        Dictionary<string, string> envVars = GetRepoEnvironmentVariables(repository);

        var envKey = key[PREFIX.Length..];

        if (envVars.TryGetValue(envKey, out var value))
        {
            return value;
        }

        return Provide(key, arg);
    }

    public object? Provide(string key, string? arg)
    {
        var envKey = key[PREFIX.Length..];
        var result = Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
        return result;
    }

    private static Dictionary<string, string> GetRepoEnvironmentVariables(IRepository repository)
    {
        return EnvironmentVariableStore.Get(repository);
    }
}