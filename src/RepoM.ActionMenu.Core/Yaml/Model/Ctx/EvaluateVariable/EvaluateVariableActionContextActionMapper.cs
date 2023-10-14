namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.EvaluateVariable;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class EvaluateVariableActionContextActionMapper : ContextActionMapperBase<EvaluateVariableContextAction>
{
    protected override async Task MapAsync(EvaluateVariableContextAction contextContextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        object? result;

        using (var _ = context.CreateGlobalScope())
        {
            result = await context.EvaluateAsync(contextContextAction.Value).ConfigureAwait(false);
        }

        scope.SetValue(contextContextAction.Name, result, false);
    }
}