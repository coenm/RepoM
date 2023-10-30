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
    public Text Name { get; set; } = null!;

    /// <summary>
    /// The text to copy to the clipboard.
    /// </summary>
    [Required]
    [Text]
    public Text Text { get; set; } = null!;
    
    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = true;

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Text}";
    }
}