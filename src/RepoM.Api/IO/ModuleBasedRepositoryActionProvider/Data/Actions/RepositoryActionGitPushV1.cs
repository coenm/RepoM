namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

/// <summary>
/// TODO
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionGitPushV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "git-push@1";
}