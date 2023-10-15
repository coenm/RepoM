namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.BrowseRepository;

using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionBrowseRepositoryV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "browse-repository@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string Name { get; init; } = string.Empty;

    public string? Active { get; init; }

    public string? FirstOnly { get; set; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}