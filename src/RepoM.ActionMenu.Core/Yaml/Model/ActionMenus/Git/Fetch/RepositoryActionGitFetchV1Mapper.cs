namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Fetch;

using System.Collections.Generic;
using RepoM.ActionMenu.Core.UserInterface;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

internal class RepositoryActionGitFetchV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionGitFetchV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionGitFetchV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);

        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = GitRepositoryCommand.Fetch,
                ExecutionCausesSynchronizing = true,
            };
    }
}