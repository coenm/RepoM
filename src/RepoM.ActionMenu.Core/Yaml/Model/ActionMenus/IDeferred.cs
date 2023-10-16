namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;

using RepoM.ActionMenu.Interface.YamlModel.Templating;

internal interface IDeferred
{
    Predicate IsDeferred { get; }
}