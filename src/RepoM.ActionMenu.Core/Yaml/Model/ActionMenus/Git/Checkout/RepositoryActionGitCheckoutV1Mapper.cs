namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;

using System;
using System.Collections.Generic;
using RepoM.ActionMenu.Core.UserInterface;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class RepositoryActionGitCheckoutV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionGitCheckoutV1>
{
    protected override IAsyncEnumerable<UserInterfaceRepositoryAction> MapAsync(RepositoryActionGitCheckoutV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        //var name = await context.RenderNullableString(action.Name).ConfigureAwait(false);

        //if (string.IsNullOrWhiteSpace(name))
        //{
        //    name = await context.TranslateAsync("Checkout").ConfigureAwait(false);
        //}

        //var text = await context.RenderStringAsync(action.Name).ConfigureAwait(false);

        // todo coenm
        throw new NotImplementedException();
    }
}