namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

/// <summary>
/// Creates a visual separator in the action menu.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionSeparatorV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "separator@1";
}