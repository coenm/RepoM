namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Url;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionUrlV1 : IMenuAction, IName, IContext
{
    public const string TYPE_VALUE = "url@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Text]
    public Text Name { get; init; } = new ScribanText();

    [Text]
    public Text Url { get; init; } = new ScribanText();

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    public Context? Context { get; init; }

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}