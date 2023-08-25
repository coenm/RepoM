namespace RepoM.Api.Git;

using System.Collections.Generic;

public interface IRepositoryActionProvider
{
    RepositoryActionBase? GetPrimaryAction(Repository repository);

    RepositoryActionBase? GetSecondaryAction(Repository repository);

    IEnumerable<RepositoryActionBase> GetContextMenuActions(Repository repository);
}