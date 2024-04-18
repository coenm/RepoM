namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class ContextActionSetVariableV1Processor : ContextActionProcessorBase<ContextActionSetVariableV1>
{
    protected override Task ProcessAsync(ContextActionSetVariableV1 contextContextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        scope.SetValue(contextContextAction.Name, contextContextAction.Value, false);
        return Task.CompletedTask;
    }
}