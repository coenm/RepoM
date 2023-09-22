namespace RepoM.Api.IO.VariableProviders;

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Plugin.Repository;
using JetBrains.Annotations;
using PluginVariableProvider = Core.Plugin.VariableProviders.IVariableProvider;
using PluginRepositoryContextVariableProvider = Core.Plugin.VariableProviders.IVariableProvider<Core.Plugin.Repository.RepositoryContext>;

[UsedImplicitly]
public class VariableProviderAdapter : ExpressionStringEvaluator.VariableProviders.IVariableProvider<RepositoryContext>
{
    private readonly PluginVariableProvider[] _variableProviderImplementation;

    public VariableProviderAdapter(IEnumerable<PluginVariableProvider> variableProviders)
    {
        _variableProviderImplementation = variableProviders.ToArray();
    }

    public bool CanProvide(string key)
    {
        return GetProvider(key) != null;
    }

    public object? Provide(string key, string? arg)
    {
        throw new NotImplementedException();
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        // Suppress nullable warning because we know that GetProvider will return a non-null value (see CanProvide)
        PluginVariableProvider instance = GetProvider(key)!;

        if (instance is PluginRepositoryContextVariableProvider typedInstance)
        {
            return typedInstance.Provide(context, key, arg);
        }

        return instance.Provide(key, arg);
    }

    private PluginVariableProvider? GetProvider(string key)
    {
        return Array.Find(_variableProviderImplementation, item => item.CanProvide(key));
    }
}