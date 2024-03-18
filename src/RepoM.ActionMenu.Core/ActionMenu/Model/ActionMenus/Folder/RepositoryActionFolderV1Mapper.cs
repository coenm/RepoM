namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Folder;

using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

[UsedImplicitly]
internal class RepositoryActionFolderV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionFolderV1>
{

    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionFolderV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await context.RenderStringAsync(action.Name).ConfigureAwait(false);

        if (action.Actions == null)
        {
            yield return new UserInterfaceRepositoryAction(name, repository);
            yield break;
        }

#pragma warning disable S2583 // Change this condition so that it does not always evaluate to 'False'. Some code paths are unreachable
        // Deferred with cloning the context doesn't work yet (https://github.com/coenm/RepoM/issues/85) therefore, set to false.
#pragma warning disable S125
        // var isDeferred = await action.IsDeferred.EvaluateAsync(context).ConfigureAwait(false);
#pragma warning restore S125
        var isDeferred = false;

        if (isDeferred)
        {
            yield return new DeferredSubActionsUserInterfaceRepositoryAction(
                name,
                repository,
                context,
                action.Actions != null,
                async ctx => await ctx.AddActionMenusAsyncArray(action.Actions).ConfigureAwait(false))
            {
                CanExecute = true,
            };
        }
        else
        {
            yield return new UserInterfaceRepositoryAction(name, repository)
            {
                CanExecute = true,
                SubActions = await context.AddActionMenusAsyncArray(action.Actions),
            };
        }
#pragma warning restore S2583
    }
}