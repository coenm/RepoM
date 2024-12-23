namespace RepoM.Plugin.AzureDevOps.ActionMenu.Model.ActionMenus.CreatePullRequest;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;

/// <summary>
/// Action menu item to create a pull request in Azure Devops. This is the same as `v1` except it will also create pull requests when there are local changes.
/// You can use the `active` property to fix this.
/// </summary>
/// <example>
/// <snippet name='azure-devops-create-pr@2-scenario01' mode='snippet' />
/// <snippet name='azure-devops-create-pr@2-scenario02' mode='snippet' />
/// <snippet name='azure-devops-create-pr@2-scenario03' mode='snippet' />
/// </example>
[RepositoryAction(TYPE_VALUE)]
internal class RepositoryActionAzureDevOpsCreatePullRequestV2 : RepositoryActionAzureDevOpsCreatePullRequestV1, IMenuAction, IContext
{
    public const string TYPE_VALUE = "azure-devops-create-pr@2";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}