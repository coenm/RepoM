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
using RepoM.Core.Plugin.Repository;

internal sealed class AzureDevOpsPullRequestService : IDisposable
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger _logger;
    private readonly VssConnection? _connection;
    private GitHttpClient? _gitClient;
    private readonly List<PullRequest> _emptyList = new(0);

    private Timer? _updateTimer1;
    private Timer? _updateTimer2;
    private readonly ConcurrentDictionary<string, Guid> _repositoryDirectoryDevOpsRepoIdMapping = new();
    private readonly ConcurrentDictionary<string, PullRequest[]> _pullRequestsPerProject = new();
    private readonly ConcurrentDictionary<string, GitRepository[]> _gitRepositoriesPerProject = new();
    private readonly ConcurrentDictionary<Guid, GitRepository> _devOpsGitRepositories = new(); // Guid is the repository guid.

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
            return Task.CompletedTask;
        }

        _updateTimer1 = new Timer(async _ => await UpdatePullRequests(_gitClient!), null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(4));
        _updateTimer2 = new Timer(async _ => await UpdateProjects(_gitClient!), null, TimeSpan.FromSeconds(7), TimeSpan.FromMinutes(10));

        return Task.CompletedTask;
    }

    private void RegisterProjectId(string? projectId)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            return;
        }

        if (!_gitRepositoriesPerProject.ContainsKey(projectId))
        {
            _gitRepositoriesPerProject.AddOrUpdate(projectId, _ => Array.Empty<GitRepository>(), (_, gitRepositories) => gitRepositories);
            _ = UpdateProjects(_gitClient!);
        }

        if (!_pullRequestsPerProject.ContainsKey(projectId))
        {
            _pullRequestsPerProject.AddOrUpdate(projectId, _ => Array.Empty<PullRequest>(), (_, prs) => prs);
            _ = UpdatePullRequests(_gitClient!);
        }
    }

    private async Task UpdateProjects(GitHttpClient gitClient)
    {
        var projectIds = _gitRepositoriesPerProject.Keys.ToArray();

        if (projectIds.Length == 0)
        {
            _logger.LogWarning("No projects for grabbing repositories.");
        }

        foreach (var projectId in projectIds)
        {
            try
            {
                List<GitRepository>? repositories = null;
                try
                {
                    _logger.LogInformation("Grabbing repositories for project {projectId}.", projectId);
                    repositories = await gitClient.GetRepositoriesAsync(projectId, includeLinks: true, includeAllUrls: true, includeHidden: true);
                }
                catch (Microsoft.TeamFoundation.Core.WebApi.ProjectDoesNotExistException e)
                {
                    _logger.LogWarning(e, "Project does not exist (projectId {projectId})", projectId);
                    //throw new ApplicationException(e.Message, e);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Unable to Get repositories from client ({projectId}).", projectId);
                    //throw new ApplicationException("Could retrieve repositories Check your PAT", e);
                }
                
                if (repositories == null || repositories.Count == 0)
                {
                    _logger.LogInformation("No repositories found for project {projectId}.", projectId);
                    _gitRepositoriesPerProject.AddOrUpdate(projectId, _ => Array.Empty<GitRepository>(), (_, _) => Array.Empty<GitRepository>());
                    continue;
                }

                _logger.LogInformation("Updating repositories {count}", projectId.Length);
                _gitRepositoriesPerProject.AddOrUpdate(projectId, _ => repositories.ToArray(), (_, _) => repositories.ToArray());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not fetch repositories for project {project}. {message}", projectId, e.Message);
                _gitRepositoriesPerProject.AddOrUpdate(projectId, _ => Array.Empty<GitRepository>(), (_, _) => Array.Empty<GitRepository>());
            }
        }
    }

    private async Task UpdatePullRequests(GitHttpClient gitClient)
    {
        var projectIds = _pullRequestsPerProject.Keys.ToArray();

        if (projectIds.Length == 0)
        {
            _logger.LogWarning("No projects for grabbing PRs.");
        }

        foreach (var projectId in projectIds)
        {
            try
            {
                _logger.LogInformation("Grabbing PRs for project {projectId}.", projectId);

                List<GitPullRequest> result = await gitClient.GetPullRequestsByProjectAsync(
                    projectId,
                    new GitPullRequestSearchCriteria
                        {
                            Status = PullRequestStatus.Active,
                            IncludeLinks = true,
                        });

                if (!result.Any())
                {
                    _logger.LogInformation("No PRs found for project {projectId}.", projectId);
                    _pullRequestsPerProject.AddOrUpdate(projectId, _ => Array.Empty<PullRequest>(), (_, _) => Array.Empty<PullRequest>());
                    continue;
                }

                _gitRepositoriesPerProject.TryGetValue(projectId, out GitRepository[]? repos);
                
                PullRequest[] pullRequests = result
                                             .Select(pr => new PullRequest(
                                                 pr.Repository.Id,
                                                 pr.Title,
                                                 CreatePullRequestUrl(repos?.SingleOrDefault(r => r.Id == pr.Repository.Id) ?? pr.Repository, pr)))
                                             .ToArray();
                _logger.LogInformation("Updating PRs {Count}", pullRequests.Length);
                _pullRequestsPerProject.AddOrUpdate(projectId, _ => pullRequests, (_, _) => pullRequests);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not fetch pull requests for project {project}. {message}", projectId, e.Message);
                _pullRequestsPerProject.AddOrUpdate(projectId, _ => Array.Empty<PullRequest>(), (_, _) => Array.Empty<PullRequest>());
            }
        }
    }

    public int CountPullRequests(IRepository repository)
    {
        var found = _repositoryDirectoryDevOpsRepoIdMapping.TryGetValue(repository.SafePath, out Guid repoIdGuid);

        if (!found)
        {
            try
            {
                repoIdGuid = FindRepositoryGuid(repository);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        if (repoIdGuid == Guid.Empty)
        {
            return 0;
        }

        return _pullRequestsPerProject.Values.Sum(prs => prs.Count(x => x.RepoId.Equals(repoIdGuid)));
    }

    public List<PullRequest> GetPullRequests(IRepository repository, string projectId, string? repoId)
    {
        RegisterProjectId(projectId);
        if (_gitRepositoriesPerProject.Count == 0)
        {
            _ = UpdateProjects(_gitClient!);
        }

        return Task.Run(() => GetPullRequestsTask(repository, projectId, repoId)).GetAwaiter().GetResult();
    }

    private async Task<List<PullRequest>> GetPullRequestsTask(IRepository repository, string projectId, string? repoId)
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
            repoIdGuid = await FindRepositoryByProjectIdx(repository, projectId);

            if (repoIdGuid == Guid.Empty)
            {
                return _emptyList;
            }

            // update mapping.
            _logger.LogTrace("Update mapping");
            _repositoryDirectoryDevOpsRepoIdMapping.AddOrUpdate(repository.SafePath, _ => repoIdGuid, (_, _) => repoIdGuid);
        }

        // at this point, we should have a repoGuid
        if (repoIdGuid == Guid.Empty)
        {
            return _emptyList;
        }

        _repositoryDirectoryDevOpsRepoIdMapping.AddOrUpdate(repository.SafePath, _ => repoIdGuid, (_, _) => repoIdGuid);

        if (_pullRequestsPerProject.TryGetValue(projectId, out PullRequest[]? projectPrs) && projectPrs.Any())
        {
            _logger.LogTrace("Returning pull requests from cache where repo id was given.");
            return projectPrs.Where(x => x.RepoId.Equals(repoIdGuid)).ToList();
        }

        _logger.LogTrace("No cache available for PRs");
        return _emptyList;
    }

    private async Task<Guid> FindRepositoryByProjectIdx(IRepository repository, string projectId)
    {
        try
        {
            List<GitRepository> repos = await _gitClient.GetRepositoriesAsync(projectId, includeLinks: true, includeAllUrls: true, includeHidden: true);

            foreach (GitRepository r in repos)
            {
                _devOpsGitRepositories.AddOrUpdate(r.Id, _ => r, (_, __) => r);
            }
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

        return FindRepositoryGuid(repository);
    }

    private Guid FindRepositoryGuid(IRepository repository)
    {
        string searchRepoUrl = GetRepositorySearchUrl(repository);

        GitRepository[] selectedRepos = _devOpsGitRepositories.Values.ToArray()
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

        return selectedRepos[0].Id;
    }


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
        _updateTimer1?.Dispose();
        _updateTimer2?.Dispose();
        _gitClient?.Dispose();
        _connection?.Dispose();
    }

    private static string CreatePullRequestUrl(GitRepository repo, GitPullRequest pr)
    {
        return repo.WebUrl + "/pullrequest/" + pr.PullRequestId;
    }

    private static string GetRepositorySearchUrl(IRepository repository)
    {
        var urlString = repository.Remotes.SingleOrDefault(x => x.Key.Equals("Origin", StringComparison.CurrentCultureIgnoreCase))?.Url ?? string.Empty;

        if (string.IsNullOrWhiteSpace(urlString))
        {
            return urlString;
        }
        
        var url = new Uri(urlString);
        var searchRepoUrl = url.Scheme + "://" + url.Host + url.LocalPath;
        return searchRepoUrl;
    }
}