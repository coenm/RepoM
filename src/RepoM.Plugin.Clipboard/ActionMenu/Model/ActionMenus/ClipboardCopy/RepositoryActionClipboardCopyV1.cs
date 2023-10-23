namespace RepoM.Plugin.Clipboard.ActionMenu.Model.ActionMenus.ClipboardCopy;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// This action makes it possible to copy text to the clipboard.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionClipboardCopyV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "clipboard-copy@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Text]
    public Text Name { get; init; } = new Text(); // todo nullable?

    /// <summary>
    /// The text to copy to the clipboard.
    /// </summary>
    [Required]
    [Text]
    public Text Text { get; set; }

    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; init; } = new Predicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Text}";
    }
}