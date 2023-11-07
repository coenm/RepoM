namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using RepoM.ActionMenu.Interface.YamlModel;

/// <summary>
/// This action will create a menu and sub menus with all local and remote branches for an easy checkout.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionGitCheckoutV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "git-checkout@1";
}