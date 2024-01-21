namespace RepoM.ActionMenu.Core.ActionMenu.Model.Command;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action to excute a command (related the the repository)
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionCommandV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "command@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IName.Name"/>
    [Text]
    public Text Name { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

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