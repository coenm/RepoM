namespace RepoM.Plugin.Clipboard.ActionProvider;

using System.ComponentModel.DataAnnotations;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// This action makes it possible to copy text to the clipboard.
/// </summary>
[RepositoryAction("clipboard-copy@1")]
public class RepositoryActionClipboardCopyV1 : RepositoryAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE = "clipboard-copy@1";

    /// <summary>
    /// The text to copy to the clipboard.
    /// </summary>
    [EvaluatedProperty]
    [Required]
    [PropertyType(typeof(string))]
    public string? Text { get; set; }
}