namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

using System;
using RepoM.ActionMenu.Interface.YamlModel;

/// <summary>
/// Action to open the default webbrowser and go to the origin remote webinterface. When multiple remotes are available a sub menu is created for each remote.
/// </summary>
[RepositoryAction(TYPE)]
[Obsolete("old menu")]
public sealed class RepositoryActionBrowseRepositoryV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "browse-repository@1";

    /// <summary>
    /// Property specifying only a menu item for the first remote is created.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    public string? FirstOnly { get; set; }
}