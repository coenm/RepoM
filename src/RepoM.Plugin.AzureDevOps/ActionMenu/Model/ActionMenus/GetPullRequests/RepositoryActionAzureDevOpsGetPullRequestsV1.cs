namespace RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.GetPullRequests;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// This action results in zero or more items in the contextmenu. For each open pullrequest for the given repository, it will show an action to go to the specific PullRequest in your favorite webbrowser.
/// </summary>
internal sealed class RepositoryActionAzureDevOpsGetPullRequestsV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "azure-devops-get-prs@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// The azure devops project id.
    /// </summary>
    [Render]
    public RenderString ProjectId { get; set; } = new RenderString(); // nullable

    /// <summary>
    /// The repository Id. If not provided, the repository id is located using the remote url.
    /// </summary>
    [EvaluatedProperty]
    [Render]
    public RenderString RepositoryId { get; set; } = new();

    /// <summary>
    /// When no pull requests are available, this property is used to determine if no or a message item is showed.
    /// </summary>
    [EvaluatedProperty]
    [EvaluateToBoolean(true)]
    public Predicate ShowWhenEmpty { get; set; } = true;
    
    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = true; // todo nullable?
    
    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}