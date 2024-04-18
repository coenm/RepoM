namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.RendererVariable;

using System.Threading.Tasks;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class ContextActionRenderVariableV1Processor : ContextActionProcessorBase<ContextActionRenderVariableV1>
{
    private ContextActionRenderVariableV1Processor()
    {
    }

    public static ContextActionRenderVariableV1Processor Instance { get; } = new();

    protected override async Task ProcessAsync(ContextActionRenderVariableV1 contextContextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        string? result;

        using (_ = context.CreateGlobalScope())
        {
            result = await context.RenderStringAsync(contextContextAction.Value).ConfigureAwait(false);
        }
        
        scope.SetValue(contextContextAction.Name, result, false);
    }
}