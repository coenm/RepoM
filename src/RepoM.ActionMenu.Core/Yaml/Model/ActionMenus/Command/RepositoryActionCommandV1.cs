namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Command;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal sealed class RepositoryActionCommandV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "command@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Render]
    public RenderString Name { get; init; } = new();

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public EvaluateBoolean Active { get; init; } = new(); // todo nullable?

    /// <summary>
    /// The command to execute.
    /// </summary>
    [Required]
    public string? Command { get; set; }

    /// <summary>
    /// Arguments for the command.
    /// </summary>
    public string? Arguments { get; set; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}