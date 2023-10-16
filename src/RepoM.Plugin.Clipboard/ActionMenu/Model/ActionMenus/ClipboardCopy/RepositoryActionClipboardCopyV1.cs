namespace RepoM.Plugin.Clipboard.ActionMenu.Model.ActionMenus.ClipboardCopy;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal sealed class RepositoryActionClipboardCopyV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "clipboard-copy@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Render]
    public RenderString Name { get; init; } = new RenderString(); // todo nullable?

    /// <summary>
    /// The text to copy to the clipboard.
    /// </summary>
    [Required]
    [Render]
    public RenderString Text { get; set; }

    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new Predicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Text}";
    }
}