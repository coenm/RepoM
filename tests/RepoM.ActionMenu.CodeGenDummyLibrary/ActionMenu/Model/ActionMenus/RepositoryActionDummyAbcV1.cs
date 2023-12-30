namespace RepoM.ActionMenu.CodeGenDummyLibrary.ActionMenu.Model.ActionMenus;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action menu item to create a pull request in Azure Devops.
/// </summary>
/// <example>
/// <usage/>
/// Get all pull requests for the selected repository in a given devops project:
/// <code>
/// devops_project_id = "805ACF64-0F06-47EC-96BF-E830895E2740";
/// prs = azure_devops.get_pull_requests(devops_project_id);
/// </code>
/// <result/>
/// As a result, the variable `prs` could contain two pull requests with the following dummy data:
/// <snippet language='yaml' name='azure-devops-create-pr@1-scenario01' mode='include' />
/// <snippet language='yaml' name='azure-devops-create-pr@1-scenario02' mode='include' />
/// <snippet language='yaml' name='azure-devops-create-pr@1-scenario03' mode='snippet' />
/// </example>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionDummyAbcV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "dummy-abc@1";

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
    public Text Name { get; set; } = null!;
    
    /// <summary>
    /// The azure devops project id.
    /// </summary>
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
    public Text PrTitle { get; set; } = null!;

    /// <summary>
    /// Name of the branch the pull request should be merged into. For instance `develop`, or `main`.
    /// </summary>
    [Text]
    public Text ToBranch { get; set; } = null!;

    /// <summary>
    /// List of reviewer ids. The id should be a valid Azure DevOps user id (ie. GUID).
    /// </summary>
    public List<Text> ReviewerIds { get; set; } = new();

    /// <summary>
    /// Boolean specifying if th PR should be marked as draft.
    /// </summary>
    [Required]
    [Predicate(false)]
    public Predicate DraftPr { get; set; } = false;

    /// <summary>
    /// Boolean specifying if workitems should be included in the PR. RepoM will try to resolve the workitems by looping through the commit messages.
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