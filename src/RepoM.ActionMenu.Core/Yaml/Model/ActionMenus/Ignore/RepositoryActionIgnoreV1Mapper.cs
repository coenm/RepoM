namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Ignore;

using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

internal class RepositoryActionIgnoreV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionIgnoreV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionIgnoreV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
        yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = IgnoreRepositoryCommand.Instance,
                ExecutionCausesSynchronizing = true,
            };
    }
}