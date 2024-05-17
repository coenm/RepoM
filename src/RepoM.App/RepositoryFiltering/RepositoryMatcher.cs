namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

internal class RepositoryMatcher : IRepositoryMatcher
{
    private readonly IQueryMatcher[] _queryMatchers;

    public RepositoryMatcher(IEnumerable<IQueryMatcher> matchers)
    {
        _queryMatchers = matchers.ToArray();
    }

    public bool Matches(IRepository repository, IQuery query)
    {
        if (query is TrueQuery)
        {
            return true;
        }

        if (query is FalseQuery)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleAnd(IRepository repository, AndQuery and)
    {
        return Array.TrueForAll(and.Items, query => Matches(repository, query));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HandleOr(IRepository repository, OrQuery or)
    {
        return Array.Exists(or.Items, query => Matches(repository, query));
    }
}