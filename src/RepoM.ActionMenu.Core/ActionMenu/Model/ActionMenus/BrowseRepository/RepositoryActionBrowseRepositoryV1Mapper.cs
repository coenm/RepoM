namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.BrowseRepository;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.ActionMenu.Interface.YamlModel;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions.Commands;

[UsedImplicitly]
internal class RepositoryActionBrowseRepositoryV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionBrowseRepositoryV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionBrowseRepositoryV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        if (repository.Remotes.Count == 0)
        {
            yield break;
        }

        var name = await context.RenderStringAsync(action.Name).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Browse remote";
        }

        var forceSingle = await action.FirstOnly.EvaluateAsync(context).ConfigureAwait(false);

        if (repository.Remotes.Count == 1 || forceSingle)
        {
            yield return new UserInterfaceRepositoryAction(name, repository)
            {
                RepositoryCommand = new BrowseRepositoryCommand(repository.Remotes[0].Url),
            };
        }
        else
        {
            yield return new DeferredSubActionsUserInterfaceRepositoryAction(
                name,
                repository,
                context,
                async ctx => await EnumerateRemotes(ctx.Repository).ConfigureAwait(false))
            {
                CanExecute = true,
            };
        }
    }

    private static Task<UserInterfaceRepositoryActionBase[]> EnumerateRemotes(IRepository repository)
    {
        return Task.FromResult(repository.Remotes
            .Take(50)
            .Select(remote => new UserInterfaceRepositoryAction(remote.Name, repository)
                {
                    RepositoryCommand = new BrowseRepositoryCommand(remote.Url),
                })
            .Cast<UserInterfaceRepositoryActionBase>()
            .ToArray());
    }
}