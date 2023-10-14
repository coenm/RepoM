namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Command;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionCommandV1 : IMenuAction, IName
{
    public const string TypeValue = "command@1";

    public string Type
    {
        get => TypeValue;
        set => _ = value;
    }

    public string Name { get; init; } = string.Empty;

    public string? Active { get; init; }

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
        return $"({TypeValue}) {Name}";
    }
}