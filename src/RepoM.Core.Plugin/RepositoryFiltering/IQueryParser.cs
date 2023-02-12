namespace RepoM.Core.Plugin.RepositoryFiltering;

using System;
using System.Linq;
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
    private readonly StringComparison _stringComparisonFreeText;
    private readonly StringComparison _stringComparisonTag;

    public FreeTextMatcher(bool ignoreCase, bool ignoreCaseTag)
    {
        _stringComparisonFreeText = ignoreCase
            ? StringComparison.CurrentCultureIgnoreCase
            : StringComparison.CurrentCulture;
        _stringComparisonTag = ignoreCaseTag
            ? StringComparison.CurrentCultureIgnoreCase
            : StringComparison.CurrentCulture;
    }

    public bool? IsMatch(IRepository repository, TermBase term)
    {
        if (term is not FreeText st)
        {
            return null;
        }

        if (repository.Tags.Any(x => x.Equals(st.Value, _stringComparisonTag)))
        {
            return true;
        }

        return repository.Name.Contains(st.Value, _stringComparisonFreeText);
    }
}