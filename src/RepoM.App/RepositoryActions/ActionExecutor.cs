namespace RepoM.App.RepositoryActions;

using System;
using RepoM.Core.Plugin;
using RepoM.Core.Plugin.RepositoryActions;
using SimpleInjector;

public sealed class ActionExecutor
{
    private readonly Container _container;

    public ActionExecutor(Container container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public void Execute(IRepository repository, IAction action)
    {
        dynamic executor = _container.GetInstance(typeof(IActionExecutor<>).MakeGenericType(action.GetType()));
        executor.Execute((dynamic)repository, (dynamic)action);
    }
}