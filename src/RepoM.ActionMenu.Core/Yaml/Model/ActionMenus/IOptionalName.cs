namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal interface IOptionalName
{
    RenderString? Name { get; }
}