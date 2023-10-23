namespace RepoM.Plugin.Heidi.ActionMenu.Model.ActionMenus.HeidiDatabases;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Action to list heidi databases and show action menus for them.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionHeidiDatabasesV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "heidi-databases@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <summary>
    /// </summary>
    [Text]
    public Text Name { get; init; } = new Text(); // todo nullable?

    /// <summary>
    /// Repository key.
    /// If not provided, the repository `Remote.Origin.Name` is used as selector.
    /// </summary>
    [Text]
    public Text Key { get; init; } = new Text(); // todo nullable?

    /// <summary>
    /// The absolute path of the Heidi executable. If not provided, the default value from the plugin settings is used.
    /// </summary>
    [Required]
    [Text]
    public Text Executable { get; init; } = new Text(); // todo nullable?

    public Context? Context { get; set; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; init; } = new Predicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Key}";
    }
}