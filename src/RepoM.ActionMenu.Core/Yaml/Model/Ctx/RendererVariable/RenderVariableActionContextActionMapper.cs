namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.RendererVariable;

using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class RenderVariableActionContextActionMapper : ContextActionMapperBase<RenderVariableContextAction>
{
    protected override async Task MapAsync(RenderVariableContextAction contextContextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        string? result;

        using (var _ = context.CreateGlobalScope())
        {
            result = await context.RenderStringAsync(contextContextAction.Value).ConfigureAwait(false);
        }
        
        scope.SetValue(contextContextAction.Name, result, false);
    }
}