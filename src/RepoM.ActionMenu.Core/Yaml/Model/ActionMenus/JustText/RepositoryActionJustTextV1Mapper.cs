namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.JustText;

using System.Collections.Generic;
using RepoM.ActionMenu.Core.UserInterface;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class RepositoryActionJustTextV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionJustTextV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryAction> MapAsync(RepositoryActionJustTextV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var text = await context.RenderStringAsync(action.Text).ConfigureAwait(false);
        yield return new UserInterfaceRepositoryAction(text, repository);
    }
}