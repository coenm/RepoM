namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using RepoM.ActionMenu.Interface.YamlModel;

/// <summary>
/// Action to execute a `git fetch` command.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionGitFetchV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "git-fetch@1";
}