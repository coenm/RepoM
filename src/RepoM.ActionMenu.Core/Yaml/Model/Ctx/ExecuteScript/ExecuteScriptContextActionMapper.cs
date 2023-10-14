namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx.ExecuteScript;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;

internal class ExecuteScriptContextActionMapper : ContextActionMapperBase<ExecuteScript>
{
    protected override async Task MapAsync(ExecuteScript contextAction, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        var result = await context.EvaluateAsync(contextAction.Content).ConfigureAwait(false);
        
        if (result is not null)
        {
            // log warning.
        }
    }
}