namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

/// <summary>
/// A visual separation in the UI menu.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionSeparatorV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "separator@1";
}