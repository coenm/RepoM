namespace RepoM.Plugin.AzureDevOps.Internal;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using RepoM.Api.IO;
using RepoM.Core.Plugin.Repository;

internal sealed class AzureDevOpsPullRequestService : IAzureDevOpsPullRequestService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IAzureDevopsConfiguration _configuration;
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

    public AzureDevOpsPullRequestService(IAzureDevopsConfiguration configuration, ILogger logger)
    {
        _httpClient = new();
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            _connection = new VssConnection(
                _configuration.AzureDevOpsBaseUrl,
                new VssBasicCredential(string.Empty, _configuration.AzureDevOpsPersonalAccessToken));
            _httpClient.BaseAddress = _configuration.AzureDevOpsBaseUrl;
            _httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_configuration.AzureDevOpsPersonalAccessToken}")));
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

        var key = _configuration.AzureDevOpsPersonalAccessToken;
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogInformation($"'{nameof(_configuration.AzureDevOpsPersonalAccessToken)}' was null or empty. Module will not be enabled.");
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

        _updateTimer1 = new Timer(async _ => await UpdatePullRequests(_azureDevopsGitClient), null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(4));
        _updateTimer2 = new Timer(async _ => await UpdateProjectsAsync(_azureDevopsGitClient), null, TimeSpan.FromSeconds(7), TimeSpan.FromMinutes(10));

        return Task.CompletedTask;
    }

    public async Task CreatePullRequestWithAutoCompleteAsync(IRepository repository, string projectId, List<string> reviewersIds, string toBranch, int mergeStrategy, string? title = null, bool isDraft = false, bool includeWorkItems = true, bool openInBrowser = false, bool deleteSourceBranch = true, bool transitionWorkItems = true, CancellationToken cancellationToken = default)
    {
        GitPullRequest? pr = await CreatePullRequestInternalAsync(repository, projectId, reviewersIds, toBranch, title, isDraft, includeWorkItems, cancellationToken);

        if (pr == null)
        {
            return;
        }

        Guid repoId = FindRepositoryGuid(repository);

        GitPullRequest prBody = new()
        {
            AutoCompleteSetBy = pr.CreatedBy,
            CompletionOptions = new()
            {
                DeleteSourceBranch = deleteSourceBranch,
                MergeStrategy = (GitPullRequestMergeStrategy)mergeStrategy,
                TransitionWorkItems = transitionWorkItems,
                MergeCommitMessage = $"Merged PR {pr.PullRequestId}: {pr.Title}",
            },
        };

        var prBodyJson = JsonConvert.SerializeObject(prBody);
        StringContent httpContent = new(prBodyJson, new MediaTypeHeaderValue("application/json"));
        HttpResponseMessage response = await _httpClient.PatchAsync($"{projectId}/_apis/git/repositories/{repoId}/pullrequests/{pr.PullRequestId}?api-version=7.0", httpContent, cancellationToken);
        _ = response.EnsureSuccessStatusCode();

        if (openInBrowser)
        {
            ProcessHelper.StartProcess(CreatePullRequestUrl(pr.Repository.WebUrl, pr.PullRequestId), string.Empty);
        }
    }

    public async Task CreatePullRequestAsync(IRepository repository, string projectId, List<string> reviewersIds, string toBranch, string? title = null, bool isDraft = false, bool includeWorkItems = true, bool openInBrowser = false, CancellationToken cancellationToken = default)
    {
        GitPullRequest? pr = await CreatePullRequestInternalAsync(repository, projectId, reviewersIds, toBranch, title, isDraft, includeWorkItems, cancellationToken);

        if (pr == null)
        {
            return;
        }

        if (openInBrowser)
        {
            ProcessHelper.StartProcess(CreatePullRequestUrl(pr.Repository.WebUrl, pr.PullRequestId), string.Empty);
        }
    }

    private async Task<GitPullRequest?> CreatePullRequestInternalAsync(IRepository repository, string projectId, List<string> reviewersIds, string toBranch, string? title = null, bool isDraft = false, bool includeWorkItems = true, CancellationToken cancellationToken = default)
    {
        title ??= repository.CurrentBranch[(repository.CurrentBranch.IndexOf('/') + 1)..];

        Guid repoId = FindRepositoryGuid(repository);

        if (repoId == Guid.Empty)
        {
            repoId = await FindRepositoryGuidByProjectId(repository, projectId);
        }

        ResourceRef[] workItems = Array.Empty<ResourceRef>();

        if (includeWorkItems)
        {
            IEnumerable<string> commitMessages = CommitMessageExtractor.GetCommitMessagesUntilBranch(repository, toBranch);

            workItems = WorkItemExtractor
                      .GetDistinctWorkItemsFromCommitMessages(commitMessages)
                      .Select(workItem => new ResourceRef { Id = workItem, })
                      .ToArray();
        }

        GitPullRequest prBody = new()
        {
            Title = title,
            IsDraft = isDraft,
            SourceRefName = $"refs/heads/{repository.CurrentBranch}",
            TargetRefName = $"refs/heads/{toBranch}",
            Reviewers = reviewersIds
                .Distinct()
                .Select(reviewerId =>
                    new IdentityRefWithVote()
                    {
                        Id = reviewerId,
                    })
                .ToArray(),
            SupportsIterations = true,
            WorkItemRefs = workItems,
        };

        var prBodyJson = JsonConvert.SerializeObject(prBody);
        StringContent httpContent = new(prBodyJson, new MediaTypeHeaderValue("application/json"));
        HttpResponseMessage response = await _httpClient.PostAsync($"{projectId}/_apis/git/repositories/{repoId}/pullrequests?api-version=7.0", httpContent, cancellationToken);
        _ = response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken) ?? throw new Exception("Invalid return type");
        GitPullRequest? result = JsonConvert.DeserializeObject<GitPullRequest>(responseContent);

        if (result == null)
        {
            _logger.LogWarning("Could not Deserialize as {Type}.", nameof(GetPullRequests));
        }

        return result;
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

        return _pullRequestsPerProject.Values.Sum(prs => prs.Count(x => x.RepositoryId.Equals(repoIdGuid)));
    }

    public List<PullRequest> GetPullRequests(IRepository repository, string projectId, string? repoId)
    {
        RegisterProjectId(projectId);
        if (_gitRepositoriesPerProject.IsEmpty)
        {
            _ = UpdateProjectsAsync(_azureDevopsGitClient);
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
            _ = UpdateProjectsAsync(_azureDevopsGitClient);
        }

        if (!_pullRequestsPerProject.ContainsKey(projectId))
        {
            _pullRequestsPerProject.AddOrUpdate(projectId, _ => Array.Empty<PullRequest>(), (_, prs) => prs);
            _ = UpdatePullRequests(_azureDevopsGitClient);
        }
    }

    private async Task UpdateProjectsAsync(GitHttpClient? gitClient)
    {
        var projectIds = _gitRepositoriesPerProject.Keys.ToArray();

        if (projectIds.Length == 0)
        {
            _logger.LogWarning("No projects for grabbing repositories.");
            return;
        }

        if (gitClient == null)
        {
            _logger.LogWarning("GitClient is null, Cannot Update projects.");
            return;
        }

        foreach (var projectId in projectIds)
        {
            try
            {
                List<GitRepository>? repositories = null;
                try
                {
                    _logger.LogInformation("Grabbing repositories for project {ProjectId}.", projectId);
                    repositories = await gitClient.GetRepositoriesAsync(projectId, includeLinks: true, includeAllUrls: true, includeHidden: true);
                }
                catch (Microsoft.TeamFoundation.Core.WebApi.ProjectDoesNotExistException e)
                {
                    _logger.LogWarning(e, "Project does not exist (projectId {ProjectId})", projectId);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Unable to Get repositories from client ({ProjectId}).", projectId);
                }

                if (repositories == null || repositories.Count == 0)
                {
                    _logger.LogInformation("No repositories found for project {ProjectId}.", projectId);
                    _gitRepositoriesPerProject.AddOrUpdate(projectId, _ => Array.Empty<GitRepository>(), (_, _) => Array.Empty<GitRepository>());
                    continue;
                }

                _logger.LogInformation("Updating repositories {Count}", projectId.Length);
                _gitRepositoriesPerProject.AddOrUpdate(projectId, _ => repositories.ToArray(), (_, _) => repositories.ToArray());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not fetch repositories for project {ProjectId}. {Message}", projectId, e.Message);
                _gitRepositoriesPerProject.AddOrUpdate(projectId, _ => Array.Empty<GitRepository>(), (_, _) => Array.Empty<GitRepository>());
            }
        }
    }

    private async Task UpdatePullRequests(GitHttpClient? gitClient)
    {
        var projectIds = _pullRequestsPerProject.Keys.ToArray();

        if (projectIds.Length == 0)
        {
            _logger.LogWarning("No projects for grabbing PRs.");
            return;
        }

        if (gitClient == null)
        {
            _logger.LogWarning("GitClient is null, Cannot Update pull requests.");
            return;
        }

        foreach (var projectId in projectIds)
        {
            try
            {
                _logger.LogInformation("Grabbing PRs for project {ProjectId}.", projectId);

                List<GitPullRequest> result = await gitClient.GetPullRequestsByProjectAsync(
                    projectId,
                    new GitPullRequestSearchCriteria
                    {
                        Status = PullRequestStatus.Active,
                        IncludeLinks = true,
                    });

                if (result.Count == 0)
                {
                    _logger.LogInformation("No PRs found for project {ProjectId}.", projectId);
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
                _logger.LogError(e, "Could not fetch pull requests for project {Project}. {Message}", projectId, e.Message);
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

        if (_pullRequestsPerProject.TryGetValue(projectId, out PullRequest[]? projectPrs) && projectPrs.Length > 0)
        {
            _logger.LogTrace("Returning pull requests from cache where repo id was given.");
            return projectPrs.Where(x => x.RepositoryId.Equals(repoIdGuid)).ToList();
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
            _logger.LogWarning(e, "Project does not exist (repository: {RepositoryName} projectId {ProjectId})", repository.Name, projectId);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Unable to Get repositories from client ({ProjectId}).", projectId);
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
            .Where(x => Array.Exists(x.ValidRemoteUrls, u => u.Equals(searchRepoUrl, StringComparison.CurrentCultureIgnoreCase)))
            .ToArray();

        if (selectedRepos.Length == 0)
        {
            _logger.LogWarning("No repository found for url {SearchRepoUrl}", searchRepoUrl);
            return Guid.Empty;
        }

        if (selectedRepos.Length > 1)
        {
            _logger.LogWarning("Multiple repositories found for url {SearchRepoUrl}", searchRepoUrl);
            return Guid.Empty;
        }

        return selectedRepos[0].Id;
    }

    private static string CreatePullRequestUrl(GitRepository repo, GitPullRequest pr)
    {
        return CreatePullRequestUrl(repo.WebUrl, pr.PullRequestId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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