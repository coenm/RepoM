namespace RepoM.ActionMenu.Core.ActionMenu.Model.ActionMenus.BrowseRepository;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

        string menuTitle;
        //var menuTitle = await context.RenderStringAsync(action.Name).ConfigureAwait(false);
        //if (string.IsNullOrWhiteSpace(menuTitle))
        //{
        //    menuTitle = "Github: ";
        //}

        var forceSingle = await action.FirstOnly.EvaluateAsync(context).ConfigureAwait(false);

        if (repository.Remotes.Count == 1 || forceSingle)
        {
            menuTitle = TryGet_RemoteRepoLongName(repository.Remotes[0].Url);
            yield return new UserInterfaceRepositoryAction(menuTitle, repository)
            {
                RepositoryCommand = new BrowseRepositoryCommand(repository.Remotes[0].Url),
            };
        }
        else
        {
            menuTitle = "Remote repos"; 
            yield return new DeferredSubActionsUserInterfaceRepositoryAction(
                menuTitle,
                repository,
                context,
                captureScope: false,
                async ctx => await EnumerateRemotes(ctx.Repository).ConfigureAwait(false))
            {
                CanExecute = true,
            };
        }
    }

    private static string TryGet_RemoteRepoLongName(string remoteUrl)
    {
        var splitString = remoteUrl.Trim().Split('/');
        if (splitString.Length < 2)
        {
            return remoteUrl;
        }

        var name = splitString.Last().Replace(".git", "");
        var author = splitString[^2];
        return $"{author}/{name}";
    }

    private static Task<UserInterfaceRepositoryActionBase[]> EnumerateRemotes(IRepository repository)
    {
        return Task.FromResult(repository.Remotes
            .Take(50)
            .Select(remote => new UserInterfaceRepositoryAction(TryGet_RemoteRepoLongName(remote.Url), repository)
            {
                RepositoryCommand = new BrowseRepositoryCommand(remote.Url),
            })
            .Cast<UserInterfaceRepositoryActionBase>()
            .ToArray());
    }
}