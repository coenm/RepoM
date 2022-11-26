namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using RepoM.Core.Plugin;
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
    public RepositoryActionBase(IRepository repository)
    {
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IAction Action { get; set; } = NullAction.Instance;

    public IRepository Repository { get; }

    public bool ExecutionCausesSynchronizing { get; set; }

    public bool CanExecute { get; set; } = true;

    public Func<RepositoryActionBase[]>? DeferredSubActionsEnumerator { get; set; }

    public IEnumerable<RepositoryActionBase>? SubActions { get; set; }
}