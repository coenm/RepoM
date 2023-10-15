namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.AssociateFile;

using RepoM.ActionMenu.Interface.YamlModel;

internal sealed class RepositoryActionAssociateFileV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "associate-file@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    public string Name { get; init; } = string.Empty;

    public string? Extension { get; init; }

    public string? Active { get; init; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Extension}";
    }
}