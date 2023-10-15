namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;

internal interface IContextActionProcessor
{
    bool CanProcess(IContextAction action);

    Task ProcessAsync(IContextAction action, IContextMenuActionMenuGenerationContext context, IScope scope);
}