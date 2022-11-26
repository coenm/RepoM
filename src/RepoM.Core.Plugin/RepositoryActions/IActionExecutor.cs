namespace RepoM.Core.Plugin.RepositoryActions;

public interface IActionExecutor<T> where T : IAction
{
    void Execute(T action);
}