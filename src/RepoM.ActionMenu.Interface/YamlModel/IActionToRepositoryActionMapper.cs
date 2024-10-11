namespace RepoM.ActionMenu.Interface.YamlModel;

using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Core.Plugin.Repository;

public interface IActionToRepositoryActionMapper
{
    bool CanMap(in IMenuAction action);

    IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(in IMenuAction action, in IActionMenuGenerationContext context, in IRepository repository);
}