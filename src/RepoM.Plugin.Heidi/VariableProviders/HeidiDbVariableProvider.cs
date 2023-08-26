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

    public object? Provide(RepositoryContext context, string key, string? arg)
    {
        var startIndex = "heidi-db".Length;
        var keySuffix = key[startIndex..];

        if (keySuffix.StartsWith(".all", StringComparison.CurrentCultureIgnoreCase))
        {
            var keySuffixInner = keySuffix[".all".Length..];
            var dbs = _service.GetAllDatabases();

            if (".count".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
            {
                return dbs.Length;
            }

            if (".any".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
            {
                return dbs.Any();
            }

            if (".empty".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
            {
                return dbs.IsEmpty;
            }

            if (".dbs".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
            {
                return dbs.ToArray();
            }
        }

        else if (keySuffix.StartsWith(".repo", StringComparison.CurrentCultureIgnoreCase))
        {
            var keySuffixInner = keySuffix[".repo".Length..];

            IRepository? repo = context.Repository;
            if (repo == null)
            {
                return null;
            }

            var dbs = _service.GetByRepository(repo);

            if (".count".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
            {
                return dbs.Count();
            }

            if (".any".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
            {
                return dbs.Any();
            }

            if (".empty".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
            {
                return !dbs.Any();
            }

            if (".dbs".Equals(keySuffixInner, StringComparison.CurrentCultureIgnoreCase))
            {
                return dbs.OrderBy(x => x.Order)
                          .ThenBy(x => x.Name)
                          .ToArray();
            }
        }

        // heidi-db[D].count

        return null;
    }

    public object? Provide(string key, string? arg)
    {
        throw new NotImplementedException();
    }
}