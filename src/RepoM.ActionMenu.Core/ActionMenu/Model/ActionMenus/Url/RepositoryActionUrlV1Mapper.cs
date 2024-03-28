namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.Url;

using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
internal class RepositoryActionUrlV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionUrlV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionUrlV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var text = await action.Name.RenderAsync(context).ConfigureAwait(false);
        var url = await action.Url.RenderAsync(context).ConfigureAwait(false);
        yield return new UserInterfaceRepositoryAction(text, repository)
            {
                RepositoryCommand = new BrowseRepositoryCommand(url),
            };
    }
}