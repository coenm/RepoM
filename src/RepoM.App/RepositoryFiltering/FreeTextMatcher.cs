namespace RepoM.App.RepositoryFiltering;

using System;
using System.Linq;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

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

    public bool? IsMatch(in IRepository repository, in TermBase term)
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