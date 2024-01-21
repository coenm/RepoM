namespace RepoM.ActionMenu.Core.ActionMenu.Model.Command;

using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
internal class RepositoryActionCommandV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionCommandV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionCommandV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
        var command = await action.Command.RenderAsync(context).ConfigureAwait(false);
        var arguments = await action.Arguments.RenderAsync(context).ConfigureAwait(false);

        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = new StartProcessRepositoryCommand(command, arguments),
            };
    }
}