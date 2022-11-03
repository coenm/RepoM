namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Collections.Generic;
using System.Linq;
using ExpressionStringEvaluator.VariableProviders;
using JetBrains.Annotations;
using RepoM.Api.IO.ExpressionEvaluator;
using RepoM.Api.IO.Variables;
using Repository = Git.Repository;

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

        var prefixLength = PREFIX.Length;
        if (key.Length <= prefixLength)
        {
            return false;
        }

        var envKey = key.Substring(prefixLength, key.Length - prefixLength);

        return !string.IsNullOrWhiteSpace(envKey);
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
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

    public object? Provide(string key, string? arg)
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