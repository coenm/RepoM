namespace RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System;
using System.Collections.Generic;
using RepoM.Api.Git;
using RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;

public interface IActionToRepositoryActionMapper
{
    bool CanMap(RepoZ.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction action);

    bool CanHandleMultipleRepositories();

    IEnumerable<RepositoryActionBase> Map(Data.RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition);
}
