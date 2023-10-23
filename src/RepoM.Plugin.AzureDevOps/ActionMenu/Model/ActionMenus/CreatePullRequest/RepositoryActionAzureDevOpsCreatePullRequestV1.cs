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
    [Text("Create Pull Request")]
    public Text Name { get; set; } = new Text(); // todo nullable?
    
    /// <summary>
    /// The azure devops project id.
    /// </summary>
    [Text]
    public Text ProjectId { get; set; } = new Text(); // nullable
    
    /// <summary>
    /// Pull Request title. When not provided, the title will be defined based on the branch name.
    /// Title will be the last part of the branchname split on `/`, so `feature/123-testBranch` will result in title `123-testBranch`
    /// </summary>
    [Text]
    public Text PrTitle { get; set; } = new Text(); // nullable

    /// <summary>
    /// Name of the branch the pull request should be merged into. For instance `develop`, or `main`.
    /// </summary>
    [Text]
    public Text ToBranch { get; set; } = new Text(); // nullable

    /// <summary>
    /// List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID).
    /// </summary>
    [PropertyType(typeof(List<string>))] 
    public List<string> ReviewerIds { get; set; } = new(); // todo List<RenderString> ?

    /// <summary>
    /// Boolean specifying if th PR should be marked as draft.
    /// </summary>
    [Required]
    [Predicate(false)]
    public Predicate DraftPr { get; set; } = false;

    /// <summary>
    /// Boolean specifying if workitems should be included in the PR. The workitems will be found by using the commit messages.
    /// </summary>
    [Required]
    [Predicate(true)]
    public Predicate IncludeWorkItems { get; set; } = true;

    /// <summary>
    /// Boolean specifying if the Pull request should be opened in the browser after creation.
    /// </summary>
    [Required]
    [Predicate(default)]
    public Predicate OpenInBrowser { get; set; } = true;

    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
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