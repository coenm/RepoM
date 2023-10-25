namespace RepoM.App.RepositoryActions;

using System;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using SimpleInjector;

public sealed class ActionExecutor
{
    private readonly Container _container;

    public ActionExecutor(Container container)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
    }

    public void Execute(IRepository repository, IRepositoryCommand repositoryCommand)
    {
        dynamic executor = _container.GetInstance(typeof(ICommandExecutor<>).MakeGenericType(repositoryCommand.GetType()));

        try
        {
            executor.Execute((dynamic)repository, (dynamic)repositoryCommand);
        }
        catch (Exception e)
        {
            // TODO
            Console.WriteLine(e);
            throw;
        }
    }
}