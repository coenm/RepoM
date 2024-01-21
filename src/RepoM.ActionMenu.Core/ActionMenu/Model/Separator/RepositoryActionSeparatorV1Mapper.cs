namespace RepoM.ActionMenu.Core.ActionMenu.Model.Separator;

using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;

[UsedImplicitly]
internal class RepositoryActionSeparatorV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionSeparatorV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionSeparatorV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        await Task.Yield();
        yield return new UserInterfaceSeparatorRepositoryAction(repository);
    }
}