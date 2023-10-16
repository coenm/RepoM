namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.AssociateFile;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal sealed class RepositoryActionAssociateFileV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "associate-file@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Render]
    public RenderString Name { get; init; } = new ScribanRenderString(); // todo nullable?

    [Render]
    public RenderString Extension { get; init; } = new ScribanRenderString(); // todo nullable?

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [EvaluateToBoolean(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name} : {Extension}";
    }
}