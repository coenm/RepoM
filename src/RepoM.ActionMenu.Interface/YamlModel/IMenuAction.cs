namespace RepoM.ActionMenu.Interface.YamlModel;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

public interface IMenuAction
{
    /// <summary>
    /// RepositoryAction type.
    /// </summary>
    string Type { get; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; }
}