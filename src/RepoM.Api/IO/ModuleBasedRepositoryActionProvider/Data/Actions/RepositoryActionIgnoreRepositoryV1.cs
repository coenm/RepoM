namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

/// <summary>
/// Action to ignore the current repository. This repository will be added to the list of ignored repositories and will never show in RepoM.
/// To undo this action, clear all ignored repositories or manually edit the ignored repositories file (when RepoM is not running).
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionIgnoreRepositoryV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "ignore-repository@1";
}