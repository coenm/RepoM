namespace RepoM.Plugin.Mcp.Tools;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Store;

[McpServerToolType]
internal static class RepositoryTools
{
    [McpServerTool(Name = "list_repositories"), Description("Lists all tracked repositories. Optionally filter by name.")]
    public static IReadOnlyList<RepositoryDto> ListRepositories(
        IRepositoryStore repositoryStore,
        [Description("Optional filter to match repository names (case-insensitive, partial match).")]
        string? nameFilter = null)
    {
        IEnumerable<RepositoryInfo> repositories = repositoryStore.Items;

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            repositories = repositories.Where(r => r.Name.Contains(nameFilter, System.StringComparison.OrdinalIgnoreCase));
        }

        return repositories
            .OrderBy(r => r.Name)
            .Select(RepositoryDto.FromRepositoryInfo)
            .ToList();
    }

    [McpServerTool(Name = "get_repository"), Description("Gets detailed information about a specific repository by its path.")]
    public static RepositoryDetailDto? GetRepository(
        IRepositoryStore repositoryStore,
        [Description("The full file system path of the repository.")]
        string path)
    {
        var safePath = path.Replace('\\', '/').TrimEnd('/').ToLowerInvariant();

        var result = repositoryStore.Lookup(safePath);
        if (result.HasValue)
        {
            return RepositoryDetailDto.FromRepositoryInfo(result.Value);
        }

        // Try matching by path as-is or by name
        var match = repositoryStore.Items.FirstOrDefault(
            r => r.Path.Equals(path, System.StringComparison.OrdinalIgnoreCase) ||
                 r.SafePath.Equals(path, System.StringComparison.OrdinalIgnoreCase) ||
                 r.SafePath.Equals(safePath, System.StringComparison.OrdinalIgnoreCase));

        return match != null ? RepositoryDetailDto.FromRepositoryInfo(match) : null;
    }

    [McpServerTool(Name = "find_repositories"), Description("Searches for repositories matching specific criteria such as branch, status, or remote URL.")]
    public static IReadOnlyList<RepositoryDto> FindRepositories(
        IRepositoryStore repositoryStore,
        [Description("Filter by current branch name (case-insensitive, partial match).")]
        string? branch = null,
        [Description("When true, only return repositories with local uncommitted changes.")]
        bool? hasLocalChanges = null,
        [Description("When true, only return repositories that are behind their remote.")]
        bool? isBehind = null,
        [Description("When true, only return repositories with unpushed commits or changes.")]
        bool? hasUnpushedChanges = null,
        [Description("Filter by remote URL (case-insensitive, partial match).")]
        string? remoteUrl = null)
    {
        IEnumerable<RepositoryInfo> repositories = repositoryStore.Items;

        if (!string.IsNullOrWhiteSpace(branch))
        {
            repositories = repositories.Where(r => r.CurrentBranch.Contains(branch, System.StringComparison.OrdinalIgnoreCase));
        }

        if (hasLocalChanges.HasValue)
        {
            repositories = repositories.Where(r => r.HasLocalChanges == hasLocalChanges.Value);
        }

        if (isBehind.HasValue)
        {
            repositories = repositories.Where(r => r.IsBehind == isBehind.Value);
        }

        if (hasUnpushedChanges.HasValue)
        {
            repositories = repositories.Where(r => r.HasUnpushedChanges == hasUnpushedChanges.Value);
        }

        if (!string.IsNullOrWhiteSpace(remoteUrl))
        {
            repositories = repositories.Where(r => r.Remotes.Any(remote => remote.Url.Contains(remoteUrl, System.StringComparison.OrdinalIgnoreCase)));
        }

        return repositories
            .OrderBy(r => r.Name)
            .Select(RepositoryDto.FromRepositoryInfo)
            .ToList();
    }
}

internal sealed class RepositoryDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("currentBranch")]
    public required string CurrentBranch { get; init; }

    [JsonPropertyName("hasLocalChanges")]
    public required bool HasLocalChanges { get; init; }

    [JsonPropertyName("hasUnpushedChanges")]
    public required bool HasUnpushedChanges { get; init; }

    [JsonPropertyName("isBehind")]
    public required bool IsBehind { get; init; }

    internal static RepositoryDto FromRepositoryInfo(RepositoryInfo repo)
    {
        return new RepositoryDto
        {
            Name = repo.Name,
            Path = repo.Path,
            CurrentBranch = repo.CurrentBranch,
            HasLocalChanges = repo.HasLocalChanges,
            HasUnpushedChanges = repo.HasUnpushedChanges,
            IsBehind = repo.IsBehind,
        };
    }
}

internal sealed class RepositoryDetailDto
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("location")]
    public required string Location { get; init; }

    [JsonPropertyName("currentBranch")]
    public required string CurrentBranch { get; init; }

    [JsonPropertyName("branches")]
    public required string[] Branches { get; init; }

    [JsonPropertyName("localBranches")]
    public required string[] LocalBranches { get; init; }

    [JsonPropertyName("tags")]
    public required string[] Tags { get; init; }

    [JsonPropertyName("remotes")]
    public required RemoteDto[] Remotes { get; init; }

    [JsonPropertyName("isBare")]
    public required bool IsBare { get; init; }

    [JsonPropertyName("hasLocalChanges")]
    public required bool HasLocalChanges { get; init; }

    [JsonPropertyName("hasUnpushedChanges")]
    public required bool HasUnpushedChanges { get; init; }

    [JsonPropertyName("isBehind")]
    public required bool IsBehind { get; init; }

    [JsonPropertyName("aheadBy")]
    public required int? AheadBy { get; init; }

    [JsonPropertyName("behindBy")]
    public required int? BehindBy { get; init; }

    [JsonPropertyName("localUntracked")]
    public required int? LocalUntracked { get; init; }

    [JsonPropertyName("localModified")]
    public required int? LocalModified { get; init; }

    [JsonPropertyName("localAdded")]
    public required int? LocalAdded { get; init; }

    [JsonPropertyName("localStaged")]
    public required int? LocalStaged { get; init; }

    [JsonPropertyName("localRemoved")]
    public required int? LocalRemoved { get; init; }

    [JsonPropertyName("stashCount")]
    public required int? StashCount { get; init; }

    [JsonPropertyName("currentBranchHasUpstream")]
    public required bool CurrentBranchHasUpstream { get; init; }

    [JsonPropertyName("currentBranchIsDetached")]
    public required bool CurrentBranchIsDetached { get; init; }

    [JsonPropertyName("currentBranchIsOnTag")]
    public required bool CurrentBranchIsOnTag { get; init; }

    internal static RepositoryDetailDto FromRepositoryInfo(RepositoryInfo repo)
    {
        return new RepositoryDetailDto
        {
            Name = repo.Name,
            Path = repo.Path,
            Location = repo.Location,
            CurrentBranch = repo.CurrentBranch,
            Branches = repo.Branches,
            LocalBranches = repo.LocalBranches,
            Tags = repo.Tags,
            Remotes = repo.Remotes.Select(r => new RemoteDto { Key = r.Key, Name = r.Name, Url = r.Url, }).ToArray(),
            IsBare = repo.IsBare,
            HasLocalChanges = repo.HasLocalChanges,
            HasUnpushedChanges = repo.HasUnpushedChanges,
            IsBehind = repo.IsBehind,
            AheadBy = repo.AheadBy,
            BehindBy = repo.BehindBy,
            LocalUntracked = repo.LocalUntracked,
            LocalModified = repo.LocalModified,
            LocalAdded = repo.LocalAdded,
            LocalStaged = repo.LocalStaged,
            LocalRemoved = repo.LocalRemoved,
            StashCount = repo.StashCount,
            CurrentBranchHasUpstream = repo.CurrentBranchHasUpstream,
            CurrentBranchIsDetached = repo.CurrentBranchIsDetached,
            CurrentBranchIsOnTag = repo.CurrentBranchIsOnTag,
        };
    }
}

internal sealed class RemoteDto
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }
}
