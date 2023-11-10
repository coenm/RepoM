namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Url;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionUrlV1 : IMenuAction, IName, IContext
{
    public const string TYPE_VALUE = "url@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IName.Name"/>
    [Text]
    public Text Name { get; set; } = null!;

    /// <summary>
    /// The URL to browse to.
    /// </summary>
    [Text]
    public Text Url { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    /// <inheritdoc cref="IContext.Context"/>
    public Context? Context { get; set; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}