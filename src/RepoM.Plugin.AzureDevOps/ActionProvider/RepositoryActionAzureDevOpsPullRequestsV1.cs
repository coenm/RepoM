namespace RepoM.Plugin.AzureDevOps.ActionProvider;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public class RepositoryActionAzureDevOpsPullRequestsV1 : RepositoryAction
{
    public string? ProjectId { get; set; }

    public string? RepoId { get; set; }

    public string? ShowWhenEmpty { get; set; }
}