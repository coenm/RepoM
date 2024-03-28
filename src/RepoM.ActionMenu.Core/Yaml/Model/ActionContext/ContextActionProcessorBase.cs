namespace RepoM.ActionMenu.Core.Yaml.Model.ActionContext;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;

public abstract class ContextActionProcessorBase<T> : IContextActionProcessor where T : IContextAction
{
    public bool CanProcess(IContextAction action)
    {
        return action is T;
    }

    public Task ProcessAsync(IContextAction action, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        return ProcessAsync((T)action, context, scope);
    }

    protected abstract Task ProcessAsync(T contextAction, IContextMenuActionMenuGenerationContext context, IScope scope);
}