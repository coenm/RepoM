namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Git.Pull;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.ActionMenus;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

/// <summary>
/// Action to execute a `git pull` command.
/// </summary>
[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionGitPullV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "git-pull@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IName.Name"/>
    [Text("🔽 Pull")]
    public Text Name { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}