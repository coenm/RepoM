namespace RepoM.Plugin.AzureDevOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using RepoM.Api.Common.Common;
using RepoM.Api.Git;

internal class AzureDevOpsPullRequestService : IDisposable
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly VssConnection? _connection;
    private GitHttpClient? _gitClient;
    // private List<Favorite> _favorites = new(0);
    private readonly List<PullRequest> _emptyList = new(0);

    public AzureDevOpsPullRequestService(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService;

        var token = _appSettingsService.AzureDevOpsPersonalAccessToken;

        try
        {
            _connection = new VssConnection(new Uri(_appSettingsService.AzureDevOpsBaseUrl), new VssBasicCredential(string.Empty, token));
        }
        catch (Exception)
        {
            // swallow for now;
        }
    }

    public Task InitializeAsync()
    {
        if (_connection == null)
        {
            return Task.CompletedTask;
        }

        var key = _appSettingsService.AzureDevOpsPersonalAccessToken;
        if (string.IsNullOrWhiteSpace(key))
        {
            return Task.CompletedTask;
        }

        try
        {
            _gitClient = _connection.GetClient<GitHttpClient>();
        }
        catch (Exception e)
        {
            throw new ApplicationException("Could not open GitClient from connection.", e);
        }

        return Task.CompletedTask;
    }

    public List<PullRequest> GetPullRequests(Repository repository, string? projectId, string? repoId)
    {
        return Task.Run(() => GetPullRequestsTask(repository, projectId, repoId)).GetAwaiter().GetResult();
    }

    private async Task<List<PullRequest>> GetPullRequestsTask(Repository repository, string? projectId, string? repoId)
    {
        if (_gitClient == null)
        {
            return _emptyList;
        }

        if (!string.IsNullOrWhiteSpace(repoId) && Guid.TryParse(repoId, out Guid repoIdGuid))
        {
            // can throw.
            GitRepository repo = await _gitClient.GetRepositoryAsync(repoIdGuid);

            List<GitPullRequest> prs = await GetPullRequests(_gitClient, repoIdGuid);
            return prs
                   .Select(pr => new PullRequest(
                       pr.Title,
                       CreatePullRequestUrl(repo, pr)))
                   .ToList();
        }

        var urlString = repository.Remotes.SingleOrDefault(x => x.Key.Equals("Origin", StringComparison.CurrentCultureIgnoreCase))?.Url ?? string.Empty;
        if (string.IsNullOrWhiteSpace(urlString))
        {
            return _emptyList;
        }

        var url = new Uri(urlString);

        List<GitRepository> repositories;
        try
        {
            repositories = await _gitClient.GetRepositoriesAsync(projectId, true, true, false);
        }
        catch (Microsoft.TeamFoundation.Core.WebApi.ProjectDoesNotExistException e)
        {
            throw new ApplicationException(e.Message, e);
        }
        catch (Exception e)
        {
            throw new ApplicationException("Could retrieve repositories Check your PAT", e);
        }

        var searchRepoUrl = url.Scheme + "://" + url.Host + url.LocalPath;

        var selectedRepos = repositories.Where(x => x.ValidRemoteUrls.Any(u => u.Equals(searchRepoUrl, StringComparison.CurrentCultureIgnoreCase))).ToArray();

        if (selectedRepos.Length == 0)
        {
            throw new ApplicationException($"No repositories found for url {searchRepoUrl}");
        }
        else if (selectedRepos.Length > 1)
        {
            throw new ApplicationException($"Multiple repositories found for url {searchRepoUrl}");
        }
        else
        {
            GitRepository repo = selectedRepos.Single();
            List<GitPullRequest> prs = await GetPullRequests(_gitClient, repo.Id);
            return prs
                   .Select(pr => new PullRequest(
                       pr.Title,
                       CreatePullRequestUrl(repo, pr)))
                   .ToList();

            // todo cache
            Console.WriteLine($"# repo id: {repo.Id}");
        }
    }

    private static Task<List<GitPullRequest>> GetPullRequests(GitHttpClientBase gitClient, Guid repoId)
    {
        return gitClient.GetPullRequestsAsync(
            repoId,
            new GitPullRequestSearchCriteria()
                {
                    Status = PullRequestStatus.Active,
                });
    }

    public void Dispose()
    {
        // _task.Dispose();
        _connection?.Dispose();
    }

    private static string CreatePullRequestUrl(GitRepository repo, GitPullRequest pr)
    {
        return repo.WebUrl + "/pullrequest/" + pr.PullRequestId;
    }
}