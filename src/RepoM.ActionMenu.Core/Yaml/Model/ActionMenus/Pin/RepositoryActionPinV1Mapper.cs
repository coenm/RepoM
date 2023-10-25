namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Pin;

using System;
using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

internal class RepositoryActionPinV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionPinV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionPinV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);

        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = CreatePinRepositoryCommand(action),
            };
    }

    private static PinRepositoryCommand CreatePinRepositoryCommand(RepositoryActionPinV1 action)
    {
        return action.Mode switch
            {
                RepositoryActionPinV1.PinMode.Toggle => PinRepositoryCommand.Toggle,
                RepositoryActionPinV1.PinMode.Pin => PinRepositoryCommand.Pin,
                RepositoryActionPinV1.PinMode.UnPin => PinRepositoryCommand.UnPin,
                _ => throw new ArgumentOutOfRangeException(action.Mode.ToString()),
            };
    }
}