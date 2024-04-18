namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.EvaluateVariable;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class ContextActionEvaluateVariableV1Processor : ContextActionProcessorBase<ContextActionEvaluateVariableV1>
{
    private ContextActionEvaluateVariableV1Processor()
    {
    }

    public static ContextActionEvaluateVariableV1Processor Instance { get; } = new();

    protected override async Task ProcessAsync(ContextActionEvaluateVariableV1 contextContextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        object? result;

        if (contextContextAction.Value == null)
        {
            scope.SetValue(contextContextAction.Name, null, false);
            return;
        }

        using (_ = context.CreateGlobalScope())
        {
            result = await contextContextAction.Value.EvaluateAsync(context).ConfigureAwait(false);
        }

        scope.SetValue(contextContextAction.Name, result, false);
    }
}