namespace RepoM.Core.Plugin.RepositoryActions;

using RepoM.Core.Plugin.Repository;

public interface ICommandExecutor<T> where T : IRepositoryCommand
{
    void Execute(IRepository repository, T action);
}