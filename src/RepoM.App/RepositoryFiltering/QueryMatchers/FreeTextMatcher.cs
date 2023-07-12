namespace RepoM.App.RepositoryFiltering.QueryMatchers;

using System;
using System.Linq;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

[UsedImplicitly]
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

        if (Array.Exists(repository.Tags, x => x.Equals(st.Value, _stringComparisonTag)))
        {
            return true;
        }

        return repository.Name.Contains(st.Value, _stringComparisonFreeText);
    }
}