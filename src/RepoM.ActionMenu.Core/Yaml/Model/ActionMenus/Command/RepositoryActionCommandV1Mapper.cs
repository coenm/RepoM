namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Command;

using System.Collections.Generic;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.UserInterface;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

internal class ActionCommandV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionCommandV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionCommandV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await context.RenderStringAsync(action.Name).ConfigureAwait(false);
        var command = await context.RenderNullableString(action.Command).ConfigureAwait(false);
        var arguments = await context.RenderNullableString(action.Arguments).ConfigureAwait(false);

        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = new StartProcessRepositoryCommand(command, arguments),
            };
    }
}