namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;

using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionGitCheckoutV1 : IMenuAction, IOptionalName
{
    public const string TypeValue = "git-checkout@1";

    public string Type
    {
        get => TypeValue;
        set => _ = value;
    }

    public string? Name { get; }
    
    public string? Active { get; init; }

    public override string ToString()
    {
        return $"({TypeValue}) {Name}";
    }
}