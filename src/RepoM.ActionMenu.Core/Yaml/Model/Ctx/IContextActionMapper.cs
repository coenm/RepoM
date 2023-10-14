namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;

internal interface IContextActionMapper
{
    bool CanMap(IContextAction action);

    Task MapAsync(IContextAction action, IContextMenuActionMenuGenerationContext context, IScope scope);
}