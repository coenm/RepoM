namespace RepoM.Api.RepositoryActions;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

public abstract class RepositoryActionBase
{
    protected RepositoryActionBase(IRepository repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IAction Action { get; init; } = NullAction.Instance;

    public IRepository Repository { get; }

    public bool ExecutionCausesSynchronizing { get; init; }

    public bool CanExecute { get; init; } = true;

    public IEnumerable<RepositoryActionBase>? SubActions { get; init; }
}