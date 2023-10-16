namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.JustText;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal sealed class RepositoryActionJustTextV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "just-text@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Render]
    public RenderString Text { get; init; } = new ScribanRenderString();

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    /// <summary>
    /// Show the menu as enabled (clickable) or disabled.
    /// </summary>
    public string? Enabled { get; init; }

    public Context? Context { get; init; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Text}";
    }
}