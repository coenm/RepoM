namespace RepoM.ActionMenu.Interface.YamlModel;

using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

public abstract class ActionToRepositoryActionMapperBase<T> : IActionToRepositoryActionMapper where T : IMenuAction
{
    public bool CanMap(IMenuAction action)
    {
        return action is T;
    }

    public IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(IMenuAction action, IActionMenuGenerationContext context, IRepository repository)
    {
        return MapAsync((T)action, context, repository);
    }

    protected abstract IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(T action, IActionMenuGenerationContext context, IRepository repository);
}