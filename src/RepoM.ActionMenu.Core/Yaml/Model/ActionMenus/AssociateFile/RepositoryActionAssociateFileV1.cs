namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.AssociateFile;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionAssociateFileV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "associate-file@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Text]
    public Text Name { get; init; } = new ScribanText(); // todo nullable?

    [Text]
    public Text Extension { get; init; } = new ScribanText(); // todo nullable?

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Extension}";
    }
}