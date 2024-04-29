namespace RepoM.Plugin.AzureDevOps.RepositoryFiltering;

using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Plugin.RepositoryFiltering;
using System;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.AzureDevOps.Internal;

internal class HasPullRequestsMatcher : IQueryMatcher
{
    private static readonly string[] _values =
        [
            "pr",
            "prs",
            "pullrequest",
            "pullrequests",
            "pull-request",
            "pull-requests",
        ];

    private readonly IAzureDevOpsPullRequestService _azureDevOpsPullRequestService;

    private readonly StringComparison _stringComparisonValue;

    public HasPullRequestsMatcher(IAzureDevOpsPullRequestService azureDevOpsPullRequestService, bool ignoreCase)
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

        if (Array.Exists(_values, x => x.Equals(st.Value, _stringComparisonValue)))
        {
            return HasPullRequests(repository);
        }

        return null;
    }

    private bool HasPullRequests(IRepository repository)
    {
        try
        {
            return _azureDevOpsPullRequestService.CountPullRequests(repository) > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}