
namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using RepoM.Api.IO.Variables;
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
    public object? Provide(string key, string? arg)
    {
        var prefixLength = PREFIX.Length;
        var envKey = key.Substring(prefixLength, key.Length - prefixLength);
        var envSearchKey = envKey;
        var index = envKey.IndexOfAny(new [] { '.', '[', });
        if (index > 0)
        {
            envSearchKey = envKey.Substring(0, index);
        }

        Scope? scope = RepoMVariableProviderStore.VariableScope.Value;

        while (true)
        {
            if (scope == null)
            {
                return null;
            }

            if (TryGetValueFromScope(scope, envSearchKey, out var result))
            {
                if (result is ExpandoObject eo)
                {
                    var selector = envKey.Substring(index, envKey.Length - index);
                    if (selector.StartsWith("."))
                    {
                        selector = selector.TrimStart('.');
                        KeyValuePair<string, object> x = eo.SingleOrDefault(pair => pair.Key == selector);
                        return x.Value;
                    }

                    return result;
                }

                return result;
            }

            scope = scope.Parent;
        }
    }

    private static bool TryGetValueFromScope(in Scope scope, string key, out object? value)
    {
        EvaluatedVariable? var = scope.Variables.FirstOrDefault(x => key.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));

        if (var != null)
        {
            value = var.Value;
            return var.Value != null;
        }

        value = null;
        return false;
    }
}