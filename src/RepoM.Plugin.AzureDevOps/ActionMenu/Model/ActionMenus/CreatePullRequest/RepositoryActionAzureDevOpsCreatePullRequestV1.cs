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
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionAzureDevOpsCreatePullRequestV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "azure-devops-create-pr@1"; // changed

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// Menu item title. When not provided, a title will be generated.
    /// This property will be used instead of the Name property.
    /// </summary>
    [Render("Create Pull Request")]
    public RenderString Name { get; set; } = new RenderString(); // todo nullable?
    
    /// <summary>
    /// The azure devops project id.
    /// </summary>
    [Render]
    public RenderString ProjectId { get; set; } = new RenderString(); // nullable
    
    /// <summary>
    /// Pull Request title. When not provided, the title will be defined based on the branch name.
    /// Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch`
    /// </summary>
    [Render]
    public RenderString PrTitle { get; set; } = new RenderString(); // nullable

    /// <summary>
    /// Name of the branch the pull request should be merged into. For instance `develop`, or `main`.
    /// </summary>
    [Render]
    public RenderString ToBranch { get; set; } = new RenderString(); // nullable

    /// <summary>
    /// List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID).
    /// </summary>
    [PropertyType(typeof(List<string>))] 
    public List<string> ReviewerIds { get; set; } = new(); // todo List<RenderString> ?

    /// <summary>
    /// Boolean specifying if th PR should be marked as draft.
    /// </summary>
    [Required]
    [EvaluateToBoolean(false)]
    public Predicate DraftPr { get; set; } = false;

    /// <summary>
    /// Boolean specifying if workitems should be included in the PR. The workitems will be found by using the commit messages.
    /// </summary>
    [Required]
    [EvaluateToBoolean(true)]
    public Predicate IncludeWorkItems { get; set; } = true;

    /// <summary>
    /// Boolean specifying if the Pull request should be opened in the browser after creation.
    /// </summary>
    [Required]
    [EvaluateToBoolean(default)]
    public Predicate OpenInBrowser { get; set; } = true;

    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = true; // todo nullable?

    /// <summary>
    /// Auto complete options. Please take a look at the same for more information
    /// </summary>
    public AutoCompleteOptionsV1? AutoComplete { get; set; }
    
    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}