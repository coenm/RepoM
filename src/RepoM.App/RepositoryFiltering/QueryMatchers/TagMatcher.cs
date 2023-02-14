namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using System.Linq;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

[UsedImplicitly]
public class TagMatcher : IQueryMatcher
{
    public bool? IsMatch(in IRepository repository, in TermBase term)
    {
        if (term is SimpleTerm st)
        {
            return IsMatch(repository, st);
        }

        if (term is StartsWithTerm swt)
        {
            return IsMatch(repository, swt);
        }

        return null;
    }

    private static bool CheckTerm(in string term)
    {
        return "tag".Equals(term, StringComparison.CurrentCulture);
    }

    private static bool? IsMatch(in IRepository repository, in StartsWithTerm term)
    {
        if (!CheckTerm(term.Term))
        {
            return null;
        }

        var value = term.Value;
        return repository.Tags.Any(tag =>
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
}