namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

public class RepositoryActionAzureDevOpsGetPullRequestsV1 : RepositoryActionAzureDevOpsBase
{
    public string? RepoId { get; set; }
    public string? ShowWhenEmpty { get; set; }
}