namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using RepoM.Api.RepositoryActions;

public interface IRepositoryActionProvider
{
    IEnumerable<RepositoryActionBase> GetContextMenuActions(Repository repository);
}