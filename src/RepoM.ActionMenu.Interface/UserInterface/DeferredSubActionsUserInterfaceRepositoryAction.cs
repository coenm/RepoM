namespace RepoM.ActionMenu.Interface.UserInterface;

using System;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.Core.Plugin.Repository;

public sealed class DeferredSubActionsUserInterfaceRepositoryAction : UserInterfaceRepositoryAction
{
    private readonly Func<IActionMenuGenerationContext, Task<UserInterfaceRepositoryActionBase[]>>? _action;
    private readonly IActionMenuGenerationContext _context;

    public DeferredSubActionsUserInterfaceRepositoryAction(string name, IRepository repository, IActionMenuGenerationContext actionMenuGenerationContext, bool captureScope)
        : base(name, repository)
    {
        _context = captureScope
            ? actionMenuGenerationContext.Clone()
            : actionMenuGenerationContext;
    }

    public Func<IActionMenuGenerationContext, Task<UserInterfaceRepositoryActionBase[]>> DeferredFunc
    {
        init => _action = value;
    }

    public async Task<UserInterfaceRepositoryActionBase[]> GetAsync()
    {
        if (_action == null)
        {
            return Array.Empty<UserInterfaceRepositoryActionBase>();
        }

        return await _action(_context).ConfigureAwait(false);
    }
}