namespace RepoM.ActionMenu.Interface.UserInterface;

using System;
using System.Collections.Generic;
using RepoM.ActionMenu.Interface.Commands;
using RepoM.Core.Plugin.Repository;

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