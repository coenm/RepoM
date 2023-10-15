namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.JustText;

using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;
using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionJustTextV1 : IMenuAction, IContext
{
    public const string TYPE_VALUE = "just-text@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string Text { get; init; }
    
    public string? Active { get; init; }

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