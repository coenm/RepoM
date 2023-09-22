namespace RepoM.Plugin.Heidi.VariableProviders;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.VariableProviders;
using RepoM.Plugin.Heidi.Interface;
using RepoM.Plugin.Heidi.Internal;
using RepoM.Plugin.Heidi.Internal.Config;

[UsedImplicitly]
internal class HeidiDbVariableProvider : IVariableProvider<RepositoryContext>
{
    private readonly IHeidiConfigurationService _service;

    public HeidiDbVariableProvider(IHeidiConfigurationService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    // heidi-db.repo.dbs => []
    // heidi-db.repo.any => bool
    // heidi-db.repo.empty => bool
    // heidi-db.repo.length => int
    // heidi-db.all.dbs => []
    // heidi-db.all.any => bool
    // heidi-db.all.empty => bool
    // heidi-db.all.length => int

    public bool CanProvide(string key)
    {
        return !string.IsNullOrWhiteSpace(key) && key.StartsWith("heidi-db.", StringComparison.CurrentCultureIgnoreCase);
    }

    public object? Provide(string key, string? arg)
    {
        throw new NotImplementedException();
    }

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        var startIndex = "heidi-db".Length;
        var keySuffix = key[startIndex..];

        if (keySuffix.StartsWith(".all", StringComparison.CurrentCultureIgnoreCase))
        {
            _ = TryProcessAllPart(keySuffix, out var result);
            return result;
        }

        if (keySuffix.StartsWith(".repo", StringComparison.CurrentCultureIgnoreCase))
        {
            _ = TryProcessRepoPart(context, keySuffix, out var result);
            return result;
        }

        // heidi-db[D].count

        return null;
    }

    private bool TryProcessRepoPart(RepositoryContext context, string keySuffix, [NotNullWhen(true)] out object? result)
    {
        var keySuffixInner = keySuffix[".repo".Length..];
        result = null;
        
        IRepository? repo = context.Repository;
        if (repo == null)
        {
            result = null;
            return false;
        }

        IEnumerable<RepositoryHeidiConfiguration> dbs = _service.GetByRepository(repo);

        if (".count".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
        {
            result = dbs.Count();
            return true;
        }

        if (".any".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
        {
            result = dbs.Any();
            return true;
        }

        if (".empty".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
        {
            result = !dbs.Any();
            return true;
        }

        if (".dbs".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
        {
            result = dbs.OrderBy(x => x.Order)
                        .ThenBy(x => x.Name)
                        .ToArray();
            return true;
        }
        
        return false;
    }

    private bool TryProcessAllPart(string keySuffix, [NotNullWhen(true)] out object? result)
    {
        var keySuffixInner = keySuffix[".all".Length..];
        ImmutableArray<HeidiSingleDatabaseConfiguration> dbs = _service.GetAllDatabases();
        result = null;

        if (".count".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
        {
            result = dbs.Length;
            return true;
        }

        if (".any".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
        {
            result = dbs.Any();
            return true;
        }

        if (".empty".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
        {
            result = dbs.IsEmpty;
            return true;
        }

        if (".dbs".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
        {
            result = dbs.ToArray();
            return true;
        }

        return false;
    }
}