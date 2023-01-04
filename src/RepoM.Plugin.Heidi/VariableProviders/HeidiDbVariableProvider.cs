namespace RepoM.Plugin.Heidi.VariableProviders;

using System;
using System.Linq;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.VariableProviders;
using RepoM.Plugin.Heidi.Internal;

[UsedImplicitly]
internal class HeidiDbVariableProvider : IVariableProvider<RepositoryContext>
{
    private readonly IHeidiConfigurationService _service;

    public HeidiDbVariableProvider(IHeidiConfigurationService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public bool CanProvide(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && key.StartsWith("heidi-db", StringComparison.CurrentCultureIgnoreCase);
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        IRepository? repo = context.Repositories.FirstOrDefault();
        if (repo == null)
        {
            return null;
        }

        if (key.Equals("heidi-db", StringComparison.CurrentCultureIgnoreCase))
        {
            return _service.GetByRepository(repo)
                           .OrderBy(x => x.Order)
                           .ThenBy(x => x.Name)
                           .ToArray();
        }
        
        var startIndex = "heidi-db".Length;
        var keySuffix = key[startIndex..];

        if (".dbs".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return _service.GetByRepository(repo)
                           .OrderBy(x => x.Order)
                           .ThenBy(x => x.Name)
                           .ToArray();
        }

        if (".count".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return _service.GetByRepository(repo).Count();
        }

        if (".any".Equals(keySuffix, StringComparison.CurrentCultureIgnoreCase))
        {
            return _service.GetByRepository(repo).Any();
        }

        // heidi-db[D].count

        return null;
    }

    public object? Provide(string key, string? arg)
    {
        throw new NotImplementedException();
    }
}