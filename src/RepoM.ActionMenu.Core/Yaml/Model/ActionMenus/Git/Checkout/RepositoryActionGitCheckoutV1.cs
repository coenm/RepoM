namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;

using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal sealed class RepositoryActionGitCheckoutV1 : IMenuAction, IOptionalName
{
    public const string TYPE_VALUE = "git-checkout@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string? Name { get; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public EvaluateBoolean Active { get; init; } = new(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}