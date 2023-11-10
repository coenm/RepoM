namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

public interface IName
{
    /// <summary>
    /// Name of the menu item.
    /// </summary>
    Text Name { get; }
}