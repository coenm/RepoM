namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Git.Push;

using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
internal class RepositoryActionGitPushV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionGitPushV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionGitPushV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);

        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = GitRepositoryCommand.Push,
                ExecutionCausesSynchronizing = true,
            };
    }
}