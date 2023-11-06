namespace RepoM.Api.RepositoryActions;

using System;
using RepoM.Api.IO.Variables;
using RepoM.Core.Plugin.Repository;

public class DeferredSubActionsRepositoryAction : RepositoryAction
{
    private readonly Func<RepositoryActionBase[]>? _action;
    private readonly Scope? _scope;

    public DeferredSubActionsRepositoryAction(string name, IRepository repository, bool captureScope)
        : base(name, repository)
    {
        if (captureScope)
        {
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
                    using IDisposable __ = _scope == null ? CreateDummyDisposable() : RepoMVariableProviderStore.Set(_scope);
                    return _action.Invoke();
                };
        }
        init => _action = value;
    }

    private static IDisposable CreateDummyDisposable()
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