namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;

using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionGitCheckoutV1 : IMenuAction, IOptionalName
{
    public const string TYPE_VALUE = "git-checkout@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string? Name { get; }
    
    public string? Active { get; init; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}