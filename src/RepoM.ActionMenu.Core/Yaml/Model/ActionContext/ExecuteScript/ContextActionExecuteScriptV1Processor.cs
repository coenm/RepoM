namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.ExecuteScript;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class ContextActionExecuteScriptV1Processor : ContextActionProcessorBase<ContextActionExecuteScriptV1>
{
    protected override async Task ProcessAsync(ContextActionExecuteScriptV1 contextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        if (string.IsNullOrWhiteSpace(contextAction.Content))
        {
            return;
        }

        var result = await context.EvaluateAsync(contextAction.Content).ConfigureAwait(false);
        
        if (result is not null)
        {
            // log warning.
        }
    }
}