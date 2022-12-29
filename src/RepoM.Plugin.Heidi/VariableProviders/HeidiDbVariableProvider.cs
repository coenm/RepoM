namespace RepoM.Plugin.Heidi.VariableProviders;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.VariableProviders;

[UsedImplicitly]
public class HeidiDbVariableProvider : IVariableProvider<RepositoryContext>
{
    public bool CanProvide(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && key.Equals("heidi-db", StringComparison.CurrentCultureIgnoreCase);
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        return null;
    }

    public object? Provide(string key, string? arg)
    {
        throw new NotImplementedException();
    }
}