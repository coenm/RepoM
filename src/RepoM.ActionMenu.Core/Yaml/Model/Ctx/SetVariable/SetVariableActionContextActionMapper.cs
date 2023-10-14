namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.SetVariable;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class SetVariableActionContextActionMapper : ContextActionMapperBase<SetVariableContextAction>
{
    protected override Task MapAsync(SetVariableContextAction contextContextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        scope.SetValue(contextContextAction.Name, contextContextAction.Value, false);
        return Task.CompletedTask;
    }
}