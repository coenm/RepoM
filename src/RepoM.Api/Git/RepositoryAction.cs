namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using RepoM.Api.IO.Variables;
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

public class DeferredRepositoryAction : RepositoryAction
{
    private readonly Func<RepositoryActionBase[]>? _action;
    private readonly Dictionary<string, string>? _envVars;
    private readonly Scope? _scope;

    public DeferredRepositoryAction(string name, IRepository repository, bool captureScope)
        : base(name, repository)
    {
        if (captureScope)
        {
            _envVars = EnvironmentVariableStore.Get(repository);
            _scope = RepoMVariableProviderStore.VariableScope.Value?.Clone();
        }
    }

    public Func<RepositoryActionBase[]> DeferredSubActionsEnumerator
    {
        get
        {
            if (_action == null)
            {
                return () => Array.Empty<RepositoryActionBase>();
            }

            return () =>
                {
                    using IDisposable _ = _envVars == null ? CreateDummy() : EnvironmentVariableStore.Set(_envVars);
                    using IDisposable __ = _scope == null ? CreateDummy() : RepoMVariableProviderStore.Set(_scope);
                    return _action.Invoke();
                };
        }
        init => _action = value;
    }

    private static IDisposable CreateDummy()
    {
        return new DummyIDisposable();
    }

    private sealed class DummyIDisposable : IDisposable
    {
        public void Dispose()
        {
            // intentionally do nothing
        }
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

    public IEnumerable<RepositoryActionBase>? SubActions { get; init; }
}