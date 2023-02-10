namespace RepoM.Core.Plugin.RepositoryFiltering;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

public interface IQueryParser
{
    IQuery Parse(string text);
}

public interface INamedQueryParser : IQueryParser
{
    string Name { get; }
}

public interface IQueryMatcher
{
    bool? IsMatch(IRepository repository, TermBase term);
}

public class IsPinnedMatcher : IQueryMatcher
{
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
            return IsPinned(repository);
        }

        if ("unpinned".Equals(st.Value, StringComparison.CurrentCulture))
        {
            return !IsPinned(repository);
        }

        return null;
    }

    private static bool IsPinned(IRepository repository)
    {
        return true;
    }
}

public class TagMatcher : IQueryMatcher
{
    public bool? IsMatch(IRepository repository, TermBase term)
    {
        if (term is not SimpleTerm st)
        {
            return null;
        }

        if (!"tag".Equals(st.Term, StringComparison.CurrentCulture))
        {
            return null;
        }

        return repository.Tags.Contains(st.Value);
    }
}

public class FreeTextMatcher : IQueryMatcher
{
    public bool? IsMatch(IRepository repository, TermBase term)
    {
        if (term is not FreeText st)
        {
            return null;
        }

        if (repository.Tags.Contains(st.Value))
        {
            return true;
        }

        return repository.Name.Contains(st.Value);
    }
}