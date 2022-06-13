namespace RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider;

using System.Collections.Generic;
using RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.ActionMappers;
using RepoM.Api.Git;
using RepositoryAction = RepoM.Api.Common.IO.ModuleBasedRepositoryActionProvider.Data.RepositoryAction;

public interface IActionToRepositoryActionMapper
{
    bool CanMap(RepositoryAction action);

    bool CanHandleMultipleRepositories();

    IEnumerable<RepositoryActionBase> Map(Data.RepositoryAction action, IEnumerable<Repository> repository, ActionMapperComposition actionMapperComposition);
}
