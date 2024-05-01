namespace RepoM.Api.RepositoryActions;

using System;
using RepoM.Core.Plugin.Repository;

public class DeferredSubActionsRepositoryAction : RepositoryAction
{
    private readonly Func<RepositoryActionBase[]>? _action;

    public DeferredSubActionsRepositoryAction(string name, IRepository repository)
        : base(name, repository)
    {
    }

    public Func<RepositoryActionBase[]> DeferredSubActionsEnumerator
    {
        get
        {
            if (_action == null)
            {
                return () => [];
            }

            return () =>
                {
                    using IDisposable __ = CreateDummyDisposable();
                    return _action.Invoke();
                };
        }
        init => _action = value;
    }

    private static DummyIDisposable CreateDummyDisposable()
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