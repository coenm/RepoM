namespace RepoM.Plugin.WebBrowser.ActionMenu.Model.ActionMenus.Browser;

using System.Collections.Generic;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.WebBrowser.RepositoryActions.Actions;

[UsedImplicitly]
internal class RepositoryActionBrowserV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionBrowserV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionBrowserV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
        var url = await action.Url.RenderAsync(context).ConfigureAwait(false);
        var profile = await action.Profile.RenderAsync(context).ConfigureAwait(false);

        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = new BrowseRepositoryCommand(url, profile),
                ExecutionCausesSynchronizing = false,
            };
    }
}