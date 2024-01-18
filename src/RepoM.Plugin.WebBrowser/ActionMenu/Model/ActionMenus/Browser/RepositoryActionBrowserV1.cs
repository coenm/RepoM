namespace RepoM.Plugin.WebBrowser.ActionMenu.Model.ActionMenus.Browser;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action opening a webbrowser with the provided url.
/// </summary>
/// <example>
/// <snippet name='webbrowser-browser@1-scenario01' mode='snippet' />
/// </example>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionBrowserV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "browser@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Text]
    public Text Name { get; init; } = null!;

    /// <summary>
    /// The url to browse to.
    /// </summary>
    [Required]
    [Text]
    public Text Url { get; set; } = null!;

    /// <summary>
    /// profile name used to select browser and browser profile
    /// </summary>
    [Required]
    [Text]
    public Text Profile { get; set; } = null!;
    
    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = true;

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Url}";
    }
}