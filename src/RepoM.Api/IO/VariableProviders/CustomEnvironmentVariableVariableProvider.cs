namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionStringEvaluator.VariableProviders;
using JetBrains.Annotations;
using RepoM.Api.IO.Variables;
using RepoM.Core.Plugin.Repository;

[UsedImplicitly]
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

        if (key.Length <= PREFIX.Length)
        {
            return false;
        }

        var envKey = key[PREFIX.Length..];

        return !string.IsNullOrWhiteSpace(envKey);
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        var envKey = key[PREFIX.Length..];

        IRepository? singleContext = context.Repositories.SingleOrDefault();

        if (singleContext == null)
        {
            return Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
        }

        Dictionary<string, string> envVars = GetRepoEnvironmentVariables(singleContext);

        if (envVars.TryGetValue(envKey, out var value))
        {
            return value;
        }

        return Environment.GetEnvironmentVariable(envKey) ?? string.Empty;
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