namespace RepoM.Plugin.AzureDevOps.Internal;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using RepoM.Api.Common;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;

internal sealed class AzureDevOpsPullRequestService : IAzureDevOpsPullRequestService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger _logger;
    private readonly VssConnection? _connection;
    private GitHttpClient? _azureDevopsGitClient;
    private readonly List<PullRequest> _emptyList = new(0);

    private Timer? _updateTimer1;
    private Timer? _updateTimer2;
    private readonly ConcurrentDictionary<string, Guid> _repositoryDirectoryDevOpsRepoIdMapping = new();
    private readonly ConcurrentDictionary<string, PullRequest[]> _pullRequestsPerProject = new();
    private readonly ConcurrentDictionary<string, GitRepository[]> _gitRepositoriesPerProject = new();
    private readonly ConcurrentDictionary<Guid, GitRepository> _devOpsGitRepositories = new(); // Guid is the repository guid.

    public AzureDevOpsPullRequestService(IAppSettingsService appSettingsService, ILogger logger)
    {
        _httpClient = new();
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var token = _appSettingsService.AzureDevOpsPersonalAccessToken;

        try
        {
            Uri baseUrl = new(_appSettingsService.AzureDevOpsBaseUrl);
            _connection = new VssConnection(
                baseUrl,
                new VssBasicCredential(string.Empty, token));
            _httpClient.BaseAddress = baseUrl;
            _httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_appSettingsService.AzureDevOpsPersonalAccessToken}")));
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
            _azureDevopsGitClient = _connection.GetClient<GitHttpClient>();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not retrieve GitHttpClient from connection.");
            return Task.CompletedTask;
        }

        _updateTimer1 = new Timer(async _ => await UpdatePullRequests(_azureDevopsGitClient!), null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(4));
        _updateTimer2 = new Timer(async _ => await UpdateProjects(_azureDevopsGitClient!), null, TimeSpan.FromSeconds(7), TimeSpan.FromMinutes(10));

        return Task.CompletedTask;
    }

    public async Task CreatePullRequestWithAutoCompleteAsync(IRepository repository, string projectId, List<string> reviewersIds, string toBranch, int mergeStrategy, string? title = null, bool isDraft = false, bool includeWorkItems = true, bool openInBrowser = false, bool deleteSourceBranch = true, bool transitionWorkItems = true, CancellationToken cancellationToken = default)
    {
        GitPullRequest pr = await CreatePullRequestInternalAsync(repository, projectId, reviewersIds, toBranch, title, isDraft, includeWorkItems, cancellationToken);

        Guid repoId = FindRepositoryGuid(repository);

        GitPullRequest prBody = new()
        {
            AutoCompleteSetBy = pr.CreatedBy,
            CompletionOptions = new()
            {
                DeleteSourceBranch = deleteSourceBranch,
                MergeStrategy = (GitPullRequestMergeStrategy)mergeStrategy,
                TransitionWorkItems = transitionWorkItems,
                MergeCommitMessage = $"Merged PR {pr.PullRequestId}: {pr.Title}"
            }
        };

        string prBodyJson = JsonConvert.SerializeObject(prBody);
        StringContent httpContent = new(prBodyJson, new MediaTypeHeaderValue("application/json"));
        HttpResponseMessage response = await _httpClient.PatchAsync($"{projectId}/_apis/git/repositories/{repoId}/pullrequests/{pr.PullRequestId}?api-version=7.0", httpContent);
        _ = response.EnsureSuccessStatusCode();

        if (openInBrowser)
        {
            ProcessHelper.StartProcess(CreatePullRequestUrl(pr.Repository.WebUrl, pr.PullRequestId), string.Empty);
        }
    }

    public async Task CreatePullRequestAsync(IRepository repository, string projectId, List<string> reviewersIds, string toBranch, string? title = null, bool isDraft = false, bool includeWorkItems = true, bool openInBrowser = false, CancellationToken cancellationToken = default)
    {
        GitPullRequest pr = await CreatePullRequestInternalAsync(repository, projectId, reviewersIds, toBranch, title, isDraft, includeWorkItems, cancellationToken);

        if (openInBrowser)
        {
            ProcessHelper.StartProcess(CreatePullRequestUrl(pr.Repository.WebUrl, pr.PullRequestId), string.Empty);
        }
    }

    private async Task<GitPullRequest> CreatePullRequestInternalAsync(IRepository repository, string projectId, List<string> reviewersIds, string toBranch, string? title = null, bool isDraft = false, bool includeWorkItems = true, CancellationToken cancellationToken = default)
    {
        title ??= repository.CurrentBranch.Substring(repository.CurrentBranch.IndexOf('/') + 1);

        Guid repoId = FindRepositoryGuid(repository);

        if (repoId == Guid.Empty)
        {
            repoId = await FindRepositoryGuidByProjectId(repository, projectId);
        }

        HashSet<ResourceRef> workItems = new();

        if (includeWorkItems)
        {
            using var repo = new LibGit2Sharp.Repository(repository.Path);

            Regex workItemRegex = new(@"\#(\d+)", RegexOptions.Compiled);

            var commitMessages = repo.Commits
                .QueryBy(new LibGit2Sharp.CommitFilter()
                {
                    ExcludeReachableFrom = repo.Branches[toBranch].UpstreamBranchCanonicalName
                })
                .Select(c => c.Message).ToList();

            foreach (var commitMessage in commitMessages)
            {
                MatchCollection matches = workItemRegex.Matches(commitMessage);
                if (matches.Success)
                {
                    foreach (System.Text.RegularExpressions.Group group in matches.Groups.Values.Skip(1))
                    {
                        _ = workItems.Add(new ResourceRef()
                        {
                            Id = group.Value
                        });
                    }
                }
            }
        }

        GitPullRequest prBody = new()
        {
            Title = title,
            IsDraft = isDraft,
            SourceRefName = $"refs/heads/{repository.CurrentBranch}",
            TargetRefName = $"refs/heads/{toBranch}",
            Reviewers = reviewersIds
                .Select(reviewerId =>
                    new IdentityRefWithVote()
                    {
                        Id = reviewerId,
                    })
                .ToArray(),
            SupportsIterations = true,
            WorkItemRefs = workItems.ToArray()
        };

        string prBodyJson = JsonConvert.SerializeObject(prBody);
        StringContent httpContent = new(prBodyJson, new MediaTypeHeaderValue("application/json"));
        HttpResponseMessage response = await _httpClient.PostAsync($"{projectId}/_apis/git/repositories/{repoId}/pullrequests?api-version=7.0", httpContent, cancellationToken);
        _ = response.EnsureSuccessStatusCode();
        
        string? responseContent = await response.Content.ReadAsStringAsync() ?? throw new Exception("Invalid return type");
        return JsonConvert.DeserializeObject<GitPullRequest>(responseContent);
    }

    public int CountPullRequests(IRepository repository)
    {
        var isRepositoryKnown = _repositoryDirectoryDevOpsRepoIdMapping.TryGetValue(repository.SafePath, out Guid repoIdGuid);

        if (!isRepositoryKnown)
        {
            try
            {
                repoIdGuid = FindRepositoryGuid(repository);

                if (repoIdGuid != Guid.Empty)
                {
                    _repositoryDirectoryDevOpsRepoIdMapping.AddOrUpdate(repository.SafePath, _ => repoIdGuid, (_, _) => repoIdGuid);
                }
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
        if (_gitRepositoriesPerProject.IsEmpty)
        {
            _ = UpdateProjects(_azureDevopsGitClient!);
        }

        return Task.Run(() => GetPullRequestsTask(repository, projectId, repoId)).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _updateTimer1?.Dispose();
        _updateTimer2?.Dispose();
        _azureDevopsGitClient?.Dispose();
        _connection?.Dispose();
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
            _ = UpdateProjects(_azureDevopsGitClient!);
        }

        if (!_pullRequestsPerProject.ContainsKey(projectId))
        {
            _pullRequestsPerProject.AddOrUpdate(projectId, _ => Array.Empty<PullRequest>(), (_, prs) => prs);
            _ = UpdatePullRequests(_azureDevopsGitClient!);
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
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Unable to Get repositories from client ({projectId}).", projectId);
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

    private async Task<List<PullRequest>> GetPullRequestsTask(IRepository repository, string projectId, string? repoId)
    {
        if (_azureDevopsGitClient == null)
        {
            return _emptyList;
        }

        Guid repoIdGuid = Guid.Empty;

        // first get repo id
        if (!string.IsNullOrWhiteSpace(repoId))
        {
            _ = Guid.TryParse(repoId, out repoIdGuid);
        }

        if (repoIdGuid == Guid.Empty && !string.IsNullOrWhiteSpace(repoId))
        {
            _ = _repositoryDirectoryDevOpsRepoIdMapping.TryGetValue(repository.SafePath, out repoIdGuid);
        }

        if (repoIdGuid == Guid.Empty)
        {
            repoIdGuid = await FindRepositoryGuidByProjectId(repository, projectId);

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

    private async Task<Guid> FindRepositoryGuidByProjectId(IRepository repository, string projectId)
    {
        try
        {
            if (_azureDevopsGitClient != null)
            {
                // yes it is possible due to a race condition that the _gitClient is null when executing this request.
                // don't care for now as we catch the exceptions.
                List<GitRepository> repos = await _azureDevopsGitClient.GetRepositoriesAsync(projectId, includeLinks: true, includeAllUrls: true, includeHidden: true);

                foreach (GitRepository r in repos)
                {
                    _devOpsGitRepositories.AddOrUpdate(r.Id, _ => r, (_, _) => r);
                }
            }
        }
        catch (Microsoft.TeamFoundation.Core.WebApi.ProjectDoesNotExistException e)
        {
            _logger.LogWarning(e, "Project does not exist (repository: {repository.Name} projectId {projectId})", repository.Name, projectId);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to Get repositories from client ({projectId}).", projectId);
        }

        return FindRepositoryGuid(repository);
    }

    private Guid FindRepositoryGuid(IRepository repository)
    {
        var searchRepoUrl = GetRepositorySearchUrl(repository);

        if (_devOpsGitRepositories.IsEmpty)
        {
            return Guid.Empty;
        }

        GitRepository[] selectedRepos = _devOpsGitRepositories.Values
            .Where(x => x.ValidRemoteUrls.Any(u => u.Equals(searchRepoUrl, StringComparison.CurrentCultureIgnoreCase)))
            .ToArray();

        if (selectedRepos.Length == 0)
        {
            _logger.LogWarning("No repository found for url {searchRepoUrl}", searchRepoUrl);
            return Guid.Empty;
        }

        if (selectedRepos.Length > 1)
        {
            _logger.LogWarning("Multiple repositories found for url {searchRepoUrl}", searchRepoUrl);
            return Guid.Empty;
        }

        return selectedRepos[0].Id;
    }

    private static string CreatePullRequestUrl(GitRepository repo, GitPullRequest pr)
    {
        return CreatePullRequestUrl(repo.WebUrl, pr.PullRequestId);
    }

    private static string CreatePullRequestUrl(string webUrl, int prId)
    {
        return $"{webUrl}/pullrequest/{prId}";
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