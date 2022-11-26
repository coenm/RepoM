namespace RepoM.Api.RepositoryActions.Decorators;

using RepoM.Core.Plugin.RepositoryActions;

public class LoggerActionExecutorDecorator<T> : IActionExecutor<T> where T : IAction
{
    private readonly IActionExecutor<T> _decoratee;

    public LoggerActionExecutorDecorator(IActionExecutor<T> decoratee)
    {
        _decoratee = decoratee;
    }

    public void Execute(T action)
    {
        _decoratee.Execute(action);
    }
}