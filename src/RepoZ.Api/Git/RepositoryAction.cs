namespace RepoZ.Api.Git;

using System;
using System.Collections.Generic;

public class RepositorySeparatorAction : RepositoryActionBase
{
}

public class RepositoryAction : RepositoryActionBase
{
    public RepositoryAction(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public abstract class RepositoryActionBase
{
    public Action<object?, object>? Action { get; set; }

    public bool ExecutionCausesSynchronizing { get; set; }

    public bool CanExecute { get; set; } = true;

    public Func<RepositoryActionBase[]>? DeferredSubActionsEnumerator { get; set; }

    public IEnumerable<RepositoryActionBase>? SubActions { get; set; }
}