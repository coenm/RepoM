namespace RepoM.App.RepositoryFiltering;

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

internal class RepositoryMatcher : IRepositoryMatcher
{
    private readonly ILogger _logger;
    private readonly IRepositoryMonitor _monitor;

    public RepositoryMatcher(ILogger logger, IRepositoryMonitor monitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public bool Matches(IRepository repository, IQuery query)
    {
        if (repository == null)
        {
            return false;
        }

        if (query == null)
        {
            return false;
        }

        if (query is AndQuery and)
        {
            return HandleAnd(repository, and);
        }

        if (query is OrQuery or)
        {
            return HandleOr(repository, or);
        }
        
        if (query is NotQuery not)
        {
            return !Matches(repository, not.Item);
        }

        if (query is SimpleTerm st)
        {
            return HandleSimpleTerm(repository, st);
        }

        return true;
    }

    private bool HandleSimpleTerm(IRepository repository, SimpleTerm st)
    {
        var x = StringComparison.InvariantCultureIgnoreCase;

        if (st.Term.Equals("is", x))
        {
            if (st.Value.Equals("pinned", x))
            {
                return _monitor.IsPinned(repository);
            }

            if (st.Value.Equals("unpinned", x))
            {
                return !_monitor.IsPinned(repository);
            }
        }

        // unknown
        return false;
    }

    private bool HandleAnd(IRepository repository, AndQuery and)
    {
        return and.Items
                  .Select(subquery => Matches(repository, subquery))
                  .All(result => result);
    }

    private bool HandleOr(IRepository repository, OrQuery or)
    {
        return or.Items
                 .Select(subquery => Matches(repository, subquery))
                 .Any(result => result);
    }
}