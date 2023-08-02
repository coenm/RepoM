namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

/// <summary>
/// Textual action to display some text in the action menu.
/// </summary>
[RepositoryAction(TYPE)]
public sealed class RepositoryActionJustTextV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "just-text@1";

    /// <summary>
    /// Show the menu as enabled (clickable) or disabled.
    /// </summary>
    [EvaluatedProperty]
    [PropertyType(typeof(bool))]
    [PropertyDefaultBoolValue(true)]
    public string? Enabled { get; set; }
}