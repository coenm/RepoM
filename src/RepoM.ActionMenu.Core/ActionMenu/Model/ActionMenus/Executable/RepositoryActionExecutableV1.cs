namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Executable;

using System.ComponentModel.DataAnnotations;
using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action to excute an application with additional arguments. This action is almost identical to the `command@1` action. When no existing executables are provided, the action will not show.
/// </summary>
/// <example>
/// <snippet name='executable@1-scenario01' mode='snippet' />
/// </example>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionExecutableV1 : IMenuAction, IName, IContext
{
    public const string TYPE_VALUE = "executable@1";
    internal const string EXAMPLE_1 = TYPE_VALUE + "-scenario01";

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
    public Predicate Active { get; init; } = new ScribanPredicate();

    /// <summary>
    /// The executable.
    /// </summary>
    [Required]
    [Text]
    public Text Executable { get; set; } = new ScribanText();

    /// <summary>
    /// Arguments for the executable.
    /// </summary>
    [Text]
    public Text Arguments { get; set; } = new ScribanText();

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; init; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}