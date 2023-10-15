namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus;

using RepoM.ActionMenu.Core.Yaml.Model.ActionContext;

internal interface IContext
{
    Context? Context { get; }
}