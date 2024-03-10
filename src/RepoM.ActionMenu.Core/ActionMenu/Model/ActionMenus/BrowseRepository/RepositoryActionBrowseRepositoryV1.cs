namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.BrowseRepository;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action to open the default webbrowser and go to the origin remote webinterface. When multiple remotes are available a sub menu is created for each remote.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionBrowseRepositoryV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "browse-repository@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IName.Name"/>
    [Text]
    public Text Name { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    /// <summary>
    /// Single menu for the first remote.
    /// </summary>
    [Predicate(false)]
    public Predicate FirstOnly { get; set; } = new ScribanPredicate();

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}