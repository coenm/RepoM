namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Pull;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data;

[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionGitPullV1 : IMenuAction, IOptionalName
{
    public const string TYPE_VALUE = "git-pull@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    [Text("Pull")]
    public Text Name { get; init; } = new ScribanText();

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; init; } = new ScribanPredicate(); // todo nullable?

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}