namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

[UsedImplicitly]
public class TagMatcher : IQueryMatcher
{
    public bool? IsMatch(in IRepository repository, in TermBase term)
    {
        return term switch
            {
                SimpleTerm st => IsMatch(repository, st),
                StartsWithTerm swt => IsMatch(repository, swt),
                _ => null,
            };
    }

    private static bool? IsMatch(in IRepository repository, in StartsWithTerm term)
    {
        if (!CheckTerm(term.Term))
        {
            return null;
        }

        var value = term.Value;
        return Array.Exists(repository.Tags, tag =>
            tag.Equals(value, StringComparison.CurrentCulture)
            ||
            tag.StartsWith(value, StringComparison.CurrentCulture));
    }

    private static bool? IsMatch(in IRepository repository, in SimpleTerm term)
    {
        if (!CheckTerm(term.Term))
        {
            return null;
        }

        return repository.Tags.Contains(term.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckTerm(in string term)
    {
        return "tag".Equals(term, StringComparison.CurrentCulture);
    }
}