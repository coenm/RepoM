namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Command;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionCommandV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "command@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Text]
    public Text Name { get; init; } = new ScribanText();

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    /// <summary>
    /// The command to execute.
    /// </summary>
    [Required]
    [Text]
    public Text Command { get; set; } = new ScribanText();

    /// <summary>
    /// Arguments for the command.
    /// </summary>
    [Text]
    public Text Arguments { get; set; } = new ScribanText();

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}