namespace RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action menu item to create a pull request in Azure Devops.
/// </summary>
/// <example>
/// <snippet name='azure-devops-create-pr@1-scenario01' mode='include' />
/// <snippet name='azure-devops-create-pr@1-scenario02' mode='include' />
/// <snippet name='azure-devops-create-pr@1-scenario03' mode='snippet' />
/// </example>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionAzureDevOpsCreatePullRequestV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "azure-devops-create-pr@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Menu item title.
    /// </summary>
    [Text("Create Pull Request")]
    public Text Name { get; set; } = null!;

    /// <summary>
    /// The azure devops project id.
    /// </summary>
    [Required]
    [Text]
    public Text ProjectId { get; set; } = null!;

    /// <summary>
    /// Pull Request title. When not provided, the title will be defined based on the branch name.
    /// Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch`
    /// </summary>
    /// <example>
    /// `{{ repository.branch | string.replace "feature/" "" | string.truncate 16 "..." }}`
    /// </example>
    [Text]
    [Required]
    public Text PrTitle { get; set; } = null!;

    /// <summary>
    /// Name of the branch the pull request should be merged into. For instance `develop`, or `main`.
    /// </summary>
    [Text]
    public Text ToBranch { get; set; } = null!;

    /// <summary>
    /// List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID).
    /// </summary>
    [PropertyType(typeof(List<string>))] 
    public List<Text> ReviewerIds { get; set; } = new();

    /// <summary>
    /// Boolean specifying if th PR should be marked as draft.
    /// </summary>
    [Predicate(false)]
    public Predicate DraftPr { get; set; } = false;

    /// <summary>
    /// Boolean specifying if workitems should be included in the PR. RepoM will try to resolve the workitems by looping through the commit messages.
    /// </summary>
    [Predicate(true)]
    public Predicate IncludeWorkItems { get; set; } = true;

    /// <summary>
    /// Boolean specifying if the Pull request should be opened in the browser after creation.
    /// </summary>
    [Predicate(false)]
    public Predicate OpenInBrowser { get; set; } = true;

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = true;

    /// <summary>
    /// Auto complete options. Please take a look at the same for more information
    /// </summary>
    public AutoCompleteOptionsV1? AutoComplete { get; set; }
    
    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}