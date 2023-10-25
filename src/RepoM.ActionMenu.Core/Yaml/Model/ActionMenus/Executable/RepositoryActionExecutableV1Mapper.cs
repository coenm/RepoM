namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Executable;

using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

internal class RepositoryActionExecutableV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionExecutableV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionExecutableV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
        var command = await action.Executable.RenderAsync(context).ConfigureAwait(false);
        var arguments = await action.Arguments.RenderAsync(context).ConfigureAwait(false);

        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = new StartProcessRepositoryCommand(command, arguments),
            };
    }
}