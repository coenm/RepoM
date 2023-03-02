namespace RepoM.Plugin.AzureDevOps.Internal;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.Core.Plugin.Repository;

internal interface IAzureDevOpsPullRequestService
{
    Task InitializeAsync();

    int CountPullRequests(IRepository repository);

    List<PullRequest> GetPullRequests(IRepository repository, string projectId, string? repoId);
}