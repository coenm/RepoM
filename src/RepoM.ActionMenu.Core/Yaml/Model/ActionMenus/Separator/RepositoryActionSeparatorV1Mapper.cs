namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Separator;

using System.Collections.Generic;
using System.Threading.Tasks;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

internal class RepositoryActionSeparatorV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionSeparatorV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionSeparatorV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        await Task.Yield();
        yield return new UserInterfaceSeparatorRepositoryAction(repository);
    }
}