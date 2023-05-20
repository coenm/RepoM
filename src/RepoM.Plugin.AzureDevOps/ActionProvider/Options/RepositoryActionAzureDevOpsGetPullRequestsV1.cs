namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public class RepositoryActionAzureDevOpsGetPullRequestsV1 : RepositoryActionAzureDevOpsBase
{
    public string? RepoId { get; set; }
    public string? ShowWhenEmpty { get; set; }
}