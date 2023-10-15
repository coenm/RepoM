namespace RepoM.ActionMenu.Interface.UserInterface;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

public abstract class UserInterfaceRepositoryActionBase
{
    protected UserInterfaceRepositoryActionBase(IRepository repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IRepositoryCommand RepositoryCommand { get; init; } = NullRepositoryCommand.Instance;

    public IRepository Repository { get; }

    public bool ExecutionCausesSynchronizing { get; init; }

    public bool CanExecute { get; init; } = true;

    public IEnumerable<UserInterfaceRepositoryActionBase>? SubActions { get; init; }
}