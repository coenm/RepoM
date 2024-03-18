namespace RepoM.ActionMenu.Interface.UserInterface;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.Core.Plugin.Repository;

public sealed class DeferredSubActionsUserInterfaceRepositoryAction : UserInterfaceRepositoryAction
{
    private readonly Func<IActionMenuGenerationContext, Task<UserInterfaceRepositoryActionBase[]>> _getFunction;
    private readonly IActionMenuGenerationContext _context;

    public DeferredSubActionsUserInterfaceRepositoryAction(
        string name,
        IRepository repository,
        IActionMenuGenerationContext actionMenuGenerationContext,
        bool captureScope,
        Func<IActionMenuGenerationContext, Task<UserInterfaceRepositoryActionBase[]>> resolveFunction)
        : base(name, repository)
    {
        _context = captureScope
            ? actionMenuGenerationContext.Clone()
            : actionMenuGenerationContext;

        _getFunction = resolveFunction;
    }

    public async Task<UserInterfaceRepositoryActionBase[]> GetAsync()
    {
        return await _getFunction(_context).ConfigureAwait(false);
    }
}