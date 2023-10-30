namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Separator;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

/// <summary>
/// Creates a visual separator in the action menu.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionSeparatorV1 : IMenuAction, IContext
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    public const string TYPE_VALUE = "separator@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    public Context? Context { get; set; }

    public override string ToString()
    {
        return $"({TYPE_VALUE})";
    }
}