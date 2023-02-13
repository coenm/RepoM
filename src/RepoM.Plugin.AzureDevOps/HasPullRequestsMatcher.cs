namespace RepoM.Plugin.AzureDevOps;

using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Plugin.RepositoryFiltering;
using System.Linq;
using System;
using RepoM.Core.Plugin.Repository;

internal class HasPullRequestsMatcher : IQueryMatcher
{
    private static readonly string[] _values =
        {
            "pr",
            "prs",
            "pullrequest",
            "pullrequests",
            "pull-request",
            "pull-requests",
        };

    private readonly AzureDevOpsPullRequestService _azureDevOpsPullRequestService;
    private readonly StringComparison _stringComparisonValue;

    public HasPullRequestsMatcher(AzureDevOpsPullRequestService azureDevOpsPullRequestService, bool ignoreCase)
    {
        _azureDevOpsPullRequestService = azureDevOpsPullRequestService ?? throw new ArgumentNullException(nameof(azureDevOpsPullRequestService));
        _stringComparisonValue = ignoreCase
            ? StringComparison.CurrentCultureIgnoreCase
            : StringComparison.CurrentCulture;
    }

    public bool? IsMatch(in IRepository repository, in TermBase term)
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
        try
        {
            var result = _azureDevOpsPullRequestService.HasPullRequests(repository);
            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
