namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.AssociateFile;

using System.Collections.Generic;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class RepositoryActionAssociateFileV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionAssociateFileV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionAssociateFileV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        if (action.Extension is null)
        {
            yield break;
        }

        // todo extension handling

        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);

        yield return new UserInterfaceRepositoryAction(name, repository);
    }
}