namespace RepoM.Plugin.AzureDevOps.ActionProvider.Options;

using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

public abstract class RepositoryActionAzureDevOpsBase : RepositoryAction
{
    public string? ProjectId { get; set; }
}