namespace RepoM.Plugin.AzureDevOps;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using RepoM.Api.Common;
using RepoM.Api.Git;

internal sealed class AzureDevOpsPullRequestService : IDisposable
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger _logger;
    private readonly VssConnection? _connection;
    private GitHttpClient? _gitClient;
    private readonly List<PullRequest> _emptyList = new(0);

    private Timer? _updateTimer;
    private readonly ConcurrentDictionary<string, Guid> _repositoryDirectoryDevOpsRepoIdMapping = new();
    private readonly ConcurrentDictionary<string, PullRequest[]> _projectIds = new();

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
            _logger.LogWarning("No connection was established to Vss. No need to initialize.");
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

        _updateTimer = new Timer(async _ => await UpdatePullRequests(_gitClient!), null, TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(10));

        return Task.CompletedTask;
    }

    private void RegisterProjectId(string? projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return;
        }

        if (_projectIds.ContainsKey(projectId))
        {
            return;
        }

        _projectIds.AddOrUpdate(projectId, _ => Array.Empty<PullRequest>(), (_, prs) => prs);
    }

    private async Task UpdatePullRequests(GitHttpClient gitClient)
    {
        var projectIds = _projectIds.Keys.ToArray();

        foreach (var projectId in projectIds)
        {
            try
            {
                List<GitPullRequest> result = await gitClient.GetPullRequestsByProjectAsync(
                    projectId,
                    new GitPullRequestSearchCriteria
                        {
                            Status = PullRequestStatus.Active,
                        });

                if (!result.Any())
                {
                    _projectIds.AddOrUpdate(projectId, _ => Array.Empty<PullRequest>(), (_, _) => Array.Empty<PullRequest>());
                    continue;
                }

                PullRequest[] pullRequests = result
                                             .Select(pr => new PullRequest(
                                                 pr.Repository.Id,
                                                 pr.Title,
                                                 CreatePullRequestUrl(pr.Repository, pr)))
                                             .ToArray() ;
                _projectIds.AddOrUpdate(projectId, _ => pullRequests, (_, _) => pullRequests);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not fetch pull requests for project {project}. {message}", projectId, e.Message);
                _projectIds.AddOrUpdate(projectId, _ => Array.Empty<PullRequest>(), (_, _) => Array.Empty<PullRequest>());
            }
        }
    }

    public List<PullRequest> GetPullRequests(Repository repository, string projectId, string? repoId)
    {
        RegisterProjectId(projectId);
        return Task.Run(() => GetPullRequestsTask(repository, projectId, repoId)).GetAwaiter().GetResult();
    }

    private async Task<List<PullRequest>> GetPullRequestsTask(Repository repository, string projectId, string? repoId)
    {
        if (_gitClient == null)
        {
            return _emptyList;
        }

        Guid repoIdGuid = Guid.Empty;

        // first get repo id
        if (repoIdGuid == Guid.Empty && !string.IsNullOrWhiteSpace(repoId))
        {
            _ = Guid.TryParse(repoId, out repoIdGuid);
        }

        if (repoIdGuid == Guid.Empty && !string.IsNullOrWhiteSpace(repoId))
        {
            _ = _repositoryDirectoryDevOpsRepoIdMapping.TryGetValue(repository.SafePath, out repoIdGuid);
        }

        if (repoIdGuid == Guid.Empty)
        {
            var urlString = repository.Remotes.SingleOrDefault(x => x.Key.Equals("Origin", StringComparison.CurrentCultureIgnoreCase))?.Url ?? string.Empty;
            if (string.IsNullOrWhiteSpace(urlString))
            {
                return _emptyList;
            }

            var url = new Uri(urlString);

            List<GitRepository> repositories;
            try
            {
                repositories = await _gitClient.GetRepositoriesAsync(projectId, includeLinks: true, includeAllUrls: true, includeHidden: true);
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

            repoIdGuid = selectedRepos.Single().Id;

            // update mapping.
            _logger.LogTrace("Update mapping");
            _repositoryDirectoryDevOpsRepoIdMapping.AddOrUpdate(repository.SafePath, _ => repoIdGuid, (_, _) => repoIdGuid);
        }

        // at this point, we should have a repoGuid
        if (repoIdGuid == Guid.Empty)
        {
            return _emptyList;
        }

        if (_projectIds.TryGetValue(projectId, out PullRequest[]? projectPrs) && projectPrs.Any())
        {
            _logger.LogTrace("Returning pull requests from cache where repo id was given.");
            return projectPrs.Where(x => x.RepoId.Equals(repoIdGuid)).ToList();
        }

        _logger.LogTrace("No cache available for PRs");
        return _emptyList;
    }

    //
    // private static Task<List<GitPullRequest>> GetPullRequests(GitHttpClientBase gitClient, Guid repoId)
    // {
    //     return gitClient.GetPullRequestsAsync(
    //         repoId,
    //         new GitPullRequestSearchCriteria
    //             {
    //                 Status = PullRequestStatus.Active,
    //             });
    // }

    public void Dispose()
    {
        _updateTimer?.Dispose();
        _gitClient?.Dispose();
        _connection?.Dispose();
    }

    private static string CreatePullRequestUrl(GitRepository repo, GitPullRequest pr)
    {
        return repo.WebUrl + "/pullrequest/" + pr.PullRequestId;
    }
}