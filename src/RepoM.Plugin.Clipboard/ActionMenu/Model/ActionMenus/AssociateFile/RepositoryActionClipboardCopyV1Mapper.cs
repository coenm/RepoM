namespace RepoM.Plugin.Clipboard.ActionMenu.Model.ActionMenus.AssociateFile;

using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Plugin.Clipboard.RepositoryAction.Actions;

internal class RepositoryActionClipboardCopyV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionClipboardCopyV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionClipboardCopyV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
        var txt = await action.Text.RenderAsync(context).ConfigureAwait(false);
        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = new CopyToClipboardRepositoryCommand(txt),
                ExecutionCausesSynchronizing = false,
            };
    }
}