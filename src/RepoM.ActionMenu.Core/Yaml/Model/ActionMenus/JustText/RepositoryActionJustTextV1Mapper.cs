namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.JustText;

using System.Collections.Generic;
using RepoM.ActionMenu.Core.UserInterface;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class RepositoryActionJustTextV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionJustTextV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionJustTextV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var text = await action.Text.RenderAsync(context).ConfigureAwait(false);
        yield return new UserInterfaceRepositoryAction(text, repository);
    }
}