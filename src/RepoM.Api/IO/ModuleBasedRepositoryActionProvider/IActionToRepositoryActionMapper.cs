namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using RepoM.Api.Git;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepositoryAction = RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public interface IActionToRepositoryActionMapper
{
    bool CanMap(RepositoryAction action);

    IEnumerable<RepositoryActionBase> Map(RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition);
}
