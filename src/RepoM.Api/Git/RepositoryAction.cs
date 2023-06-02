namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Actions;

public class RepositorySeparatorAction : RepositoryActionBase
{
    public RepositorySeparatorAction(IRepository repository)
        : base(repository)
    {
    }
}


public class RepositoryAction : RepositoryActionBase
{
    public RepositoryAction(string name, IRepository repository):
        base(repository)
    {
        Name = name;
    }

    public string Name { get; }
}

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

    public Func<RepositoryActionBase[]>? DeferredSubActionsEnumerator { get; init; }

    public IEnumerable<RepositoryActionBase>? SubActions { get; init; }
}