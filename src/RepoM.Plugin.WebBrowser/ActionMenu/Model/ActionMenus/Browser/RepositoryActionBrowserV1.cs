namespace RepoM.Plugin.WebBrowser.ActionMenu.Model.ActionMenus.Browser;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action opening a webbrowser with the provided url.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionBrowserV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "browser@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Render]
    public RenderString Name { get; init; } = new RenderString(); // todo nullable?

    /// <summary>
    /// The url to browse to.
    /// </summary>
    [Required]
    [Render]
    public RenderString Url { get; init; } = new RenderString(); // todo nullable?

    /// <summary>
    /// profile name used to select browser and browser profile
    /// </summary>
    [Required]
    [Render]
    public RenderString Profile { get; set; } = new RenderString(); // todo nullable?

    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new Predicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Url}";
    }
}