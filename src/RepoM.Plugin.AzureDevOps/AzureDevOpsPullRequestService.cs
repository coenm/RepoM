namespace RepoM.Plugin.AzureDevOps;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using RepoM.Api.Common;
using RepoM.Api.Git;

internal class AzureDevOpsPullRequestService : IDisposable
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger _logger;
    private readonly VssConnection? _connection;
    private GitHttpClient? _gitClient;
    private readonly List<PullRequest> _emptyList = new(0);

    public AzureDevOpsPullRequestService(
        IAppSettingsService appSettingsService,
        ILogger logger)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var token = _appSettingsService.AzureDevOpsPersonalAccessToken;

        try
        {
            _connection = new VssConnection(
                new Uri(_appSettingsService.AzureDevOpsBaseUrl),
                new VssBasicCredential(string.Empty, token));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not connect to Vss. Module will not be enabled.");
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
            _logger.LogInformation($"'{nameof(_appSettingsService.AzureDevOpsPersonalAccessToken)}' was null or empty. Module will not be enabled.");
            return Task.CompletedTask;
        }

        try
        {
            _gitClient = _connection.GetClient<GitHttpClient>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not retrieve GitHttpClient from connection.");
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
                       repoIdGuid,
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
            repositories = await _gitClient.GetRepositoriesAsync(projectId, includeLinks:true, includeAllUrls:true, includeHidden:true);
        }
        catch (Microsoft.TeamFoundation.Core.WebApi.ProjectDoesNotExistException e)
        {
            _logger.LogWarning(e, "Project does not exist (repository: {repository.Name} projectId {projectId})", repository.Name, projectId);
            throw new ApplicationException(e.Message, e);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to Get repositories from client ({projectId}).", projectId);
            throw new ApplicationException("Could retrieve repositories Check your PAT", e);
        }

        var searchRepoUrl = url.Scheme + "://" + url.Host + url.LocalPath;

        GitRepository[] selectedRepos = repositories
                                        .Where(x => x.ValidRemoteUrls.Any(u => u.Equals(searchRepoUrl, StringComparison.CurrentCultureIgnoreCase)))
                                        .ToArray();

        if (selectedRepos.Length == 0)
        {
            _logger.LogWarning("No repository found for url {searchRepoUrl}", searchRepoUrl);
            throw new ApplicationException($"No repositories found for url {searchRepoUrl}");
        }

        if (selectedRepos.Length > 1)
        {
            _logger.LogWarning("Multiple repositories found for url {searchRepoUrl}", searchRepoUrl);
            throw new ApplicationException($"Multiple repositories found for url {searchRepoUrl}");
        }

        {
            GitRepository repo = selectedRepos.Single();
            List<GitPullRequest> prs = await GetPullRequests(_gitClient, repo.Id);
            return prs
                   .Select(pr => new PullRequest(
                       repo.Id,
                       pr.Title,
                       CreatePullRequestUrl(repo, pr)))
                   .ToList();
        }
    }

    private static Task<List<GitPullRequest>> GetPullRequests(GitHttpClientBase gitClient, Guid repoId)
    {
        return gitClient.GetPullRequestsAsync(
            repoId,
            new GitPullRequestSearchCriteria
                {
                    Status = PullRequestStatus.Active,
                });
    }

    public void Dispose()
    {
        _gitClient?.Dispose();
        _connection?.Dispose();
    }

    private static string CreatePullRequestUrl(GitRepository repo, GitPullRequest pr)
    {
        return repo.WebUrl + "/pullrequest/" + pr.PullRequestId;
    }
}