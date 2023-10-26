namespace RepoM.ActionMenu.Core.Yaml.Model.ActionMenus.Git.Checkout;

using System;
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
internal class RepositoryActionGitCheckoutV1Mapper : ActionToRepositoryActionMapperBase<RepositoryActionGitCheckoutV1>
{
    protected override async IAsyncEnumerable<UserInterfaceRepositoryActionBase> MapAsync(RepositoryActionGitCheckoutV1 action, IActionMenuGenerationContext context, IRepository repository)
    {
        var name = await action.Name.RenderAsync(context).ConfigureAwait(false);
        var remoteBranchesTranslated = await context.TranslateAsync("Remote branches");
        var noRemoteBranchesFoundTranslated = await context.TranslateAsync("No remote branches found");
        var tryFetchChangesTranslated = await context.TranslateAsync("Try to fetch changes if you're expecting remote branches");

        yield return new DeferredSubActionsUserInterfaceRepositoryAction(name, repository, context, false)
        {
            DeferredFunc = _ =>
                Task.FromResult(repository.LocalBranches
                          .Take(50)
                          .Select(branch => new UserInterfaceRepositoryAction(branch, repository)
                          {
                              RepositoryCommand = GitRepositoryCommand.Checkout(branch),
                              CanExecute = !repository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                          })
                          .Union(new UserInterfaceRepositoryActionBase[]
                              {
                                      new UserInterfaceSeparatorRepositoryAction(repository),
                                      new DeferredSubActionsUserInterfaceRepositoryAction(remoteBranchesTranslated, repository, context, false)
                                          {
                                              DeferredFunc = _ =>
                                                  {
                                                      UserInterfaceRepositoryActionBase[] remoteBranches = repository
                                                                                                           .ReadAllBranches()
                                                                                                           .Select(branch => new UserInterfaceRepositoryAction(branch, repository)
                                                                                                               {
                                                                                                                   RepositoryCommand = GitRepositoryCommand.Checkout(branch),
                                                                                                                   CanExecute = !repository.CurrentBranch.Equals(branch, StringComparison.OrdinalIgnoreCase),
                                                                                                               })
                                                                                                           .ToArray();

                                                      if (remoteBranches.Any())
                                                      {
                                                          return Task.FromResult(remoteBranches);
                                                      }

                                                      var errorMenu = new UserInterfaceRepositoryActionBase[]
                                                          {
                                                              new UserInterfaceRepositoryAction(noRemoteBranchesFoundTranslated, repository)
                                                                  {
                                                                      CanExecute = false,
                                                                  },
                                                              new UserInterfaceRepositoryAction(tryFetchChangesTranslated, repository)
                                                                  {
                                                                      CanExecute = false,
                                                                  },
                                                          };
                                                      return Task.FromResult(errorMenu);
                                                  },
                                          },
                              })
                          .ToArray()),
        };
    }
}