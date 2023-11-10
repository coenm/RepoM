namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;

using RepoM.ActionMenu.Core.Yaml.Model.Templating;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.ActionMenu.Interface.YamlModel.Templating;

[RepositoryAction(TYPE_VALUE)]
internal sealed class RepositoryActionGitCheckoutV1 : IMenuAction, IName
{
    public const string TYPE_VALUE = "git-checkout@1";

    public string Type
    {
        get => TYPE_VALUE;
        set => _ = value;
    }

    /// <inheritdoc cref="IName.Name"/>
    [Text("Checkout")]
    public Text Name { get; set; } = null!;

    /// <inheritdoc cref="IMenuAction.Active"/>
    [Predicate(true)]
    public Predicate Active { get; set; } = new ScribanPredicate();

    public override string ToString()
    {
        return $"({TYPE_VALUE}) {Name}";
    }
}