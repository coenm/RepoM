namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Repositories.Monitoring;

[UsedImplicitly]
public sealed class IsMonitoredMatcher : IQueryMatcher
{
    private readonly IRepositoryMonitoringService _monitoringService;

    public IsMonitoredMatcher(IRepositoryMonitoringService monitoringService)
    {
        _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
    }

    public bool? IsMatch(in IRepository repository, in TermBase term)
    {
        if (term is not SimpleTerm st)
        {
            return null;
        }

        if (!"is".Equals(st.Term, StringComparison.Ordinal))
        {
            return null;
        }

        if ("active".Equals(st.Value, StringComparison.Ordinal))
        {
            return _monitoringService.IsMonitored(repository.SafePath);
        }

        if ("inactive".Equals(st.Value, StringComparison.Ordinal))
        {
            return !_monitoringService.IsMonitored(repository.SafePath);
        }

        return null;
    }
}
