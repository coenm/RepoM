namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

/// <summary>
/// Action to execute a `git pull` command.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionGitPullV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "git-pull@1";
}