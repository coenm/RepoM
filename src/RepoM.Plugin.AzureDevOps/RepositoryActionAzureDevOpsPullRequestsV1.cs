namespace RepoM.Plugin.AzureDevOps;

using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data;

public class RepositoryActionAzureDevOpsPullRequestsV1 : RepositoryAction
{
    public string? OrganisationUrl { get; set; }

    public string? ProjectId { get; set; }

    public string? RepoId { get; set; }

    public bool ShowWhenEmpty { get; set; }
}