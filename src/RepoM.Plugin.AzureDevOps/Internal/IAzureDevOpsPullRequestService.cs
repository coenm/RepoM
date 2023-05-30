namespace RepoM.Plugin.AzureDevOps.Internal;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;

internal interface IAzureDevOpsPullRequestService
{
    Task InitializeAsync();

    int CountPullRequests(IRepository repository);

    Task CreatePullRequestWithAutoCompleteAsync(IRepository repository, string projectId, List<string> reviewersIds, string toBranch, int mergeStrategy, string? title = null, bool isDraft = false, bool includeWorkItems = true, bool openInBrowser = false, bool deleteSourceBranch = true, bool transitionWorkItems = true, CancellationToken cancellationToken = default);

    Task CreatePullRequestAsync(IRepository repository, string projectId, List<string> reviewersIds, string toBranch, string? title = null, bool isDraft = false, bool includeWorkItems = true, bool openInBrowser = false, CancellationToken cancellationToken = default);

    List<PullRequest> GetPullRequests(IRepository repository, string projectId, string? repoId);
}