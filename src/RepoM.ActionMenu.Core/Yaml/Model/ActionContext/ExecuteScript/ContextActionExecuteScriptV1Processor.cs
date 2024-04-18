namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext.ExecuteScript;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class ContextActionExecuteScriptV1Processor : ContextActionProcessorBase<ContextActionExecuteScriptV1>
{
    private ContextActionExecuteScriptV1Processor()
    {
    }

    public static ContextActionExecuteScriptV1Processor Instance { get; } = new();

    protected override async Task ProcessAsync(ContextActionExecuteScriptV1 contextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        if (contextAction.Content == null)
        {
            return;
        }

        object? result = await contextAction.Content.EvaluateAsync(context).ConfigureAwait(false);
        
        if (result is not null)
        {
            // log warning.
        }
    }
}