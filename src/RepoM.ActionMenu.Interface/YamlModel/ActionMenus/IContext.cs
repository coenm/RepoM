namespace RepoM.ActionMenu.Interface.YamlModel.ActionMenus;

public interface IContext
{
    /// <summary>
    /// The context in which the action is available.
    /// </summary>
    Context? Context { get; }
}