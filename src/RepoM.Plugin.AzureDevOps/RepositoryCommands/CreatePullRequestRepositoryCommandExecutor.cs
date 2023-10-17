namespace RepoM.Plugin.AzureDevOps.RepositoryCommands;

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Plugin.AzureDevOps.Internal;

[UsedImplicitly]
internal class CreatePullRequestRepositoryCommandExecutor : ICommandExecutor<CreatePullRequestRepositoryCommand>
{
    private readonly IAzureDevOpsPullRequestService _service;

    public CreatePullRequestRepositoryCommandExecutor(IAzureDevOpsPullRequestService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void Execute(IRepository repository, CreatePullRequestRepositoryCommand repositoryCommand)
    {
        ExecuteAsync(repository, repositoryCommand).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private async Task ExecuteAsync(IRepository repository, CreatePullRequestRepositoryCommand repositoryCommand)
    {
        if (repositoryCommand.AutoComplete == null)
        {
            await _service.CreatePullRequestAsync(
                repository,
                repositoryCommand.ProjectId,
                repositoryCommand.ReviewerIds,
                repositoryCommand.ToBranch,
                repositoryCommand.PullRequestTitle,
                repositoryCommand.Draft,
                repositoryCommand.IncludeWorkItems,
                repositoryCommand.OpenInBrowser).ConfigureAwait(false);
        }
        else
        {
            await _service.CreatePullRequestWithAutoCompleteAsync(
                repository,
                repositoryCommand.ProjectId,
                repositoryCommand.ReviewerIds,
                repositoryCommand.ToBranch,
                (int)repositoryCommand.AutoComplete.MergeStrategy,
                repositoryCommand.PullRequestTitle,
                repositoryCommand.Draft,
                repositoryCommand.IncludeWorkItems,
                repositoryCommand.OpenInBrowser,
                repositoryCommand.AutoComplete.DeleteSourceBranch,
                repositoryCommand.AutoComplete.TransitionWorkItems).ConfigureAwait(false);
        }
    }
}