namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.EvaluateVariable;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class ContextActionEvaluateVariableV1Processor : ContextActionProcessorBase<ContextActionEvaluateVariableV1>
{
    protected override async Task ProcessAsync(ContextActionEvaluateVariableV1 contextContextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        object? result;

        using (_ = context.CreateGlobalScope())
        {
            result = await context.EvaluateAsync(contextContextAction.Value).ConfigureAwait(false);
        }

        scope.SetValue(contextContextAction.Name, result, false);
    }
}