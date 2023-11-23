namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Folder;

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

        var isDeferred = await action.IsDeferred.EvaluateAsync(context).ConfigureAwait(false);

        // todo
        isDeferred = false;

        if (isDeferred)
        {
            yield return new DeferredSubActionsUserInterfaceRepositoryAction(name, repository, context, action.Actions != null)
            {
                CanExecute = true,
                DeferredFunc = async ctx => await ctx.AddActionMenusAsyncArray(action.Actions).ConfigureAwait(false),
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
    }
}