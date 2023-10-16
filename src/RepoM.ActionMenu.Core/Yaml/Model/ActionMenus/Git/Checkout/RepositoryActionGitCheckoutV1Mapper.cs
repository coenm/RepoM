namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;

using System.Collections.Generic;
using RepoM.ActionMenu.Core.UserInterface;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class RepositoryActionGitCheckoutV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionGitCheckoutV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryAction> MapAsync(RepositoryActionGitCheckoutV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
        
        // todo implement
        yield return new UserInterfaceRepositoryAction(name, repository);
    }
}