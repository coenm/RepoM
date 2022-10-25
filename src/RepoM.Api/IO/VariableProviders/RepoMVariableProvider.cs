using RepoM.Api.IO.Variables;

namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Linq;
using ExpressionStringEvaluator.Methods;
using ExpressionStringEvaluator.VariableProviders;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

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
    public CombinedTypeContainer Provide(string key, string? arg)
    {
        var prefixLength = PREFIX.Length;
        var envKey = key.Substring(prefixLength, key.Length - prefixLength);

        Scope? scope = RepoMVariableProviderStore.VariableScope.Value;

        while (true)
        {
            if (scope == null)
            {
                return CombinedTypeContainer.NullInstance;
            }

            if (TryGetValueFromScope(scope, envKey, out var result))
            {
                return result;
            }

            scope = scope.Parent;
        }
    }

    private static bool TryGetValueFromScope(in Scope scope, string key, out CombinedTypeContainer value)
    {
        EvaluatedVariable? var = scope.Variables.FirstOrDefault(x => key.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));

        if (var != null)
        {
            if (var.Value == CombinedTypeContainer.NullInstance)
            {
                value = CombinedTypeContainer.NullInstance;
                return false;
            }

            value = var.Value;
            return true;
        }

        value = CombinedTypeContainer.NullInstance;
        return false;
    }
}