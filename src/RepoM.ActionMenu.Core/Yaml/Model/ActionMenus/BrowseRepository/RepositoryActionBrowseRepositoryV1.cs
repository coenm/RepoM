namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.BrowseRepository;

using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionBrowseRepositoryV1 : IMenuAction, IName
{
    public const string TypeValue = "browse-repository@1";

    public string Type
    {
        get => TypeValue;
        set => _ = value;
    }

    public string Name { get; init; } = string.Empty;

    public string? Active { get; init; }

    public string? FirstOnly { get; set; }

    public override string ToString()
    {
        return $"({TypeValue}) {Name}";
    }
}