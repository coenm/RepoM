namespace RepoM.ActionMenu.Core.Yaml.Model.Ctx;

using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;

public abstract class ContextActionMapperBase<T> : IContextActionMapper where T : IContextAction
{
    public bool CanMap(IContextAction action)
    {
        return action is T;
    }

    public Task MapAsync(IContextAction action, IContextMenuActionMenuGenerationContext context, IScope scope)
    {
        return MapAsync((T)action, context, scope);
    }

    protected abstract Task MapAsync(T contextAction, IContextMenuActionMenuGenerationContext context, IScope scope);
}