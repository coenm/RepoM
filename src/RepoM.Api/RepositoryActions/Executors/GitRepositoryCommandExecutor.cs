namespace RepoM.Api.RepositoryActions.Executors;

using System;
using JetBrains.Annotations;
using RepoM.Api.Git;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
public sealed class GitRepositoryCommandExecutor : ICommandExecutor<GitRepositoryCommand>
{
    private readonly IRepositoryWriter _repositoryWriter;

    public GitRepositoryCommandExecutor(IRepositoryWriter repositoryWriter)
    {
        _repositoryWriter = repositoryWriter ?? throw new ArgumentNullException(nameof(repositoryWriter));
    }

    public void Execute(IRepository repository, GitRepositoryCommand repositoryCommand)
    {
        switch (repositoryCommand.GitAction)
        {
            case GitRepositoryCommand.GitActionType.Fetch:
                _repositoryWriter.Fetch(repository);
                break;
            case GitRepositoryCommand.GitActionType.Pull:
                _repositoryWriter.Pull(repository);
                break;
            case GitRepositoryCommand.GitActionType.Push:
                _repositoryWriter.Push(repository);
                break;
            case GitRepositoryCommand.GitActionType.Checkout:
                _repositoryWriter.Checkout(repository, repositoryCommand.Branch);
                break;
            default:
                throw new ArgumentOutOfRangeException($"GitAction {repositoryCommand.GitAction} not understood.");
        }
    }
}