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
        return query switch
        {
            TrueQuery => true,
            FalseQuery => false,
            AndQuery and => HandleAnd(repository, and),
            OrQuery or => HandleOr(repository, or),
            NotQuery not => !Matches(repository, not.Item),
            TermBase st => HandleTerm(repository, st),
            _ => true,
        };
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