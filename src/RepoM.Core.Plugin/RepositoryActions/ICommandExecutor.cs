namespace RepoM.Core.Plugin.RepositoryActions;

using RepoM.Core.Plugin.Repository;

// ReSharper disable once TypeParameterCanBeVariant  Justification: https://docs.simpleinjector.org/en/latest/advanced.html#covariance-contravariance
public interface ICommandExecutor<T> where T : IRepositoryCommand
{
    void Execute(IRepository repository, T action);
}