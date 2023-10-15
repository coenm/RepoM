namespace RepoM.Core.Plugin.RepositoryActions;

using RepoM.Core.Plugin.Repository;

public interface IActionExecutor<T> where T : IRepositoryCommand
{
    void Execute(IRepository repository, T action);
}