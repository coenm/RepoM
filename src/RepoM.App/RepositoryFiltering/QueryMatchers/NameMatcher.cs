namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

[UsedImplicitly]
public class NameMatcher : IQueryMatcher
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

        return repository.Name.StartsWith(term.Value, StringComparison.CurrentCulture);
    }

    private static bool? IsMatch(in IRepository repository, in SimpleTerm term)
    {
        if (!CheckTerm(term.Term))
        {
            return null;
        }

        return repository.Name.Equals(term.Value, StringComparison.CurrentCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckTerm(in string term)
    {
        return "name".Equals(term, StringComparison.CurrentCulture);
    }
}