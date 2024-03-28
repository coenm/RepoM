namespace RepoM.Api.RepositoryActions.Decorators;

using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;

public class LoggerCommandExecutorDecorator<T> : ICommandExecutor<T> where T : IRepositoryCommand
{
    private readonly ICommandExecutor<T> _decoratee;

    public LoggerCommandExecutorDecorator(ICommandExecutor<T> decoratee)
    {
        _decoratee = decoratee;
    }

    public void Execute(IRepository repository, T action)
    {
        _decoratee.Execute(repository, action);
    }
}