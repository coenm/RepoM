namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

public class HasPullRequestsMatcher : IQueryMatcher
{
    private static readonly string[] _values = new[]
        {
            "pr",
            "prs",
            "pullrequest",
            "pullrequests",
            "pull-request",
            "pull-requests",
        };

    private readonly StringComparison _stringComparisonValue;

    public HasPullRequestsMatcher(bool ignoreCase)
    {
        _stringComparisonValue = ignoreCase
            ? StringComparison.CurrentCultureIgnoreCase
            : StringComparison.CurrentCulture;
    }

    public bool? IsMatch(IRepository repository, TermBase term)
    {
        if (term is not SimpleTerm st)
        {
            return null;
        }

        if (!"has".Equals(st.Term, StringComparison.CurrentCulture))
        {
            return null;
        }

        if (_values.Any(x => x.Equals(st.Value, _stringComparisonValue)))
        {
            return HasPullRequests(repository);
        }

        return null;
    }

    private bool HasPullRequests(IRepository repository)
    {
        return false;
    }
}

public class HasUnPushedChangesMatcher : IQueryMatcher
{
    private static readonly string[] _values =
        {
            "unpushed",
            "unpushed-changes",
        };

    public bool? IsMatch(IRepository repository, TermBase term)
    {
        if (term is not SimpleTerm st)
        {
            return null;
        }

        if (!"has".Equals(st.Term, StringComparison.CurrentCulture))
        {
            return null;
        }

        if (_values.Contains(st.Value))
        {
            return repository.HasUnpushedChanges;
        }

        return null;
    }
}


public class IsPinnedMatcher : IQueryMatcher
{
    private readonly IRepositoryMonitor _monitor;

    public IsPinnedMatcher(IRepositoryMonitor monitor)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
    }

    public bool? IsMatch(IRepository repository, TermBase term)
    {
        if (term is not SimpleTerm st)
        {
            return null;
        }

        if (!"is".Equals(st.Term, StringComparison.CurrentCulture))
        {
            return null;
        }

        if ("pinned".Equals(st.Value, StringComparison.CurrentCulture))
        {
            return _monitor.IsPinned(repository);
        }

        if ("unpinned".Equals(st.Value, StringComparison.CurrentCulture))
        {
            return !_monitor.IsPinned(repository);
        }

        return null;
    }
}


internal class RepositoryMatcher : IRepositoryMatcher
{
    private readonly ILogger _logger;
    private readonly IList<IQueryMatcher> _queryMatchers;

    public RepositoryMatcher(ILogger logger, IRepositoryMonitor monitor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _queryMatchers = new List<IQueryMatcher>
            {
                new IsPinnedMatcher(monitor),
                new TagMatcher(),
                new HasPullRequestsMatcher(ignoreCase:true),
                new HasUnPushedChangesMatcher(),
                new FreeTextMatcher(ignoreCase:true, ignoreCaseTag:true),
            };
    }

    public bool Matches(IRepository repository, IQuery query)
    {
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

        if (query is TermBase st)
        {
            return HandleTerm(repository, st);
        }

        return true;
    }

    private bool HandleTerm(IRepository repository, TermBase termBase)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (IQueryMatcher matcher in _queryMatchers)
        {
            var result = matcher.IsMatch(repository, termBase);
            if (result.HasValue)
            {
                return result.Value;
            }
        }

        // unknown
        return false;
    }

    private bool HandleAnd(IRepository repository, AndQuery and)
    {
        return and.Items
                  .Select(q => Matches(repository, q))
                  .All(result => result);
    }

    private bool HandleOr(IRepository repository, OrQuery or)
    {
        return or.Items
                 .Select(q => Matches(repository, q))
                 .Any(result => result);
    }
}