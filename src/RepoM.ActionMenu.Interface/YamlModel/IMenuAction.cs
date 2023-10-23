namespace RepoM.ActionMenu.Interface.YamlModel;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

public interface IMenuAction
{
    string Type { get; }

    /// <summary>
    /// Whether the menu item is enabled.
    /// </summary>
    [Predicate(true)]
    public Predicate Active { get; }
}