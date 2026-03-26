namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Reading;

public class LibGit2SharpRepositoryInfoReader : IRepositoryInfoReader
{
    private readonly IRepositoryTagsFactory _resolver;
    private readonly ILogger _logger;

    public LibGit2SharpRepositoryInfoReader(IRepositoryTagsFactory resolver, ILogger logger)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RepositoryInfo?> ReadAsync(string path, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var repoPath = LibGit2Sharp.Repository.Discover(path);
        if (string.IsNullOrEmpty(repoPath))
        {
            _logger.LogWarning("Could not Discover git repo in path {Path}", path);
            return null;
        }

        RepositoryInfo? result = await ReadWithRetries(repoPath, 3).ConfigureAwait(false);
        if (result != null)
        {
            // Create a temporary adapter to pass to the tags resolver
            var tempAdapter = new RepoM.Core.Repositories.Adapters.RepositoryInfoAdapter(result);
            var tags = (await _resolver.GetTagsAsync(tempAdapter).ConfigureAwait(false)).ToArray();
            result.Tags = tags;
        }
        else
        {
            _logger.LogWarning("Could not read git repo in path {Path}", repoPath);
        }

        return result;
    }

    private async Task<RepositoryInfo?> ReadWithRetries(string repoPath, int maxRetries)
    {
        RepositoryInfo? info = null;
        var currentTry = 1;

        while (info == null && currentTry <= maxRetries)
        {
            try
            {
                info = ReadInternal(repoPath);
            }
            catch (LockedFileException e)
            {
                _logger.LogWarning(e, "LockedFileException {Path}", repoPath);

                if (currentTry >= maxRetries)
                {
                    throw;
                }

                await Task.Delay(500).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception when reading repo {Path}. {Message}", repoPath, e.Message);
                throw;
            }

            currentTry++;
        }

        return info;
    }

    private RepositoryInfo? ReadInternal(string repoPath)
    {
        try
        {
            using var repo = new LibGit2Sharp.Repository(repoPath);

            RepositoryStatus? status = null;
            var workingDirectory = new DirectoryInfo(repoPath);

            if (!repo.Info.IsBare)
            {
                status = repo.RetrieveStatus(new StatusOptions
                    {
                        IncludeIgnored = false,
                        DetectRenamesInIndex = false,
                        DetectRenamesInWorkDir = false,
                    });
                workingDirectory = new DirectoryInfo(repo.Info.WorkingDirectory);
            }

            if (string.IsNullOrWhiteSpace(workingDirectory.Parent?.FullName))
            {
                _logger.LogError("WorkingDirectory.Parent.Fullname was null or empty for repository found in '{Path}'. Return null", repoPath);
                return null;
            }

            HeadDetails headDetails = GetHeadDetails(repo);
            var fullPath = workingDirectory.FullName;

            var allBranchList = new List<string>();
            var localBranchList = new List<string>();
            foreach (Branch branch in repo.Branches)
            {
                allBranchList.Add(branch.FriendlyName);
                if (!branch.IsRemote)
                {
                    localBranchList.Add(branch.FriendlyName);
                }
            }

            var allBranchNames = allBranchList.ToArray();
            var localBranchNames = localBranchList.ToArray();

            int? localUntracked = null;
            int? localModified = null;
            int? localMissing = null;
            int? localAdded = null;
            int? localStaged = null;
            int? localRemoved = null;

            if (status is not null)
            {
                localUntracked = status.Untracked.Count();
                localModified = status.Modified.Count();
                localMissing = status.Missing.Count();
                localAdded = status.Added.Count();
                localStaged = status.Staged.Count();
                localRemoved = status.Removed.Count();
            }

            var info = new RepositoryInfo
                {
                    Path = fullPath,
                    SafePath = GetSafePath(fullPath),
                    WindowsPath = GetWindowsPath(fullPath),
                    LinuxPath = GetSafePath(fullPath),
                    Name = workingDirectory.Name,
                    Location = workingDirectory.Parent!.FullName,
                    IsBare = repo.Info.IsBare,
                    Branches = allBranchNames,
                    LocalBranches = localBranchNames,
                    AllBranchesReader = () => ReadAllBranches(repoPath),
                    CurrentBranch = headDetails.Name,
                    CurrentBranchHasUpstream = !string.IsNullOrEmpty(repo.Head.UpstreamBranchCanonicalName),
                    CurrentBranchIsDetached = headDetails.IsDetached,
                    CurrentBranchIsOnTag = headDetails.IsOnTag,
                    AheadBy = repo.Head.TrackingDetails?.AheadBy,
                    BehindBy = repo.Head.TrackingDetails?.BehindBy,
                    LocalUntracked = localUntracked,
                    LocalModified = localModified,
                    LocalMissing = localMissing,
                    LocalAdded = localAdded,
                    LocalStaged = localStaged,
                    LocalRemoved = localRemoved,
                    StashCount = repo.Stashes?.Count() ?? 0,
                    Tags = [],
                };

            RemoteCollection? remoteCollection = repo.Network?.Remotes;
            if (remoteCollection != null)
            {
                foreach (LibGit2Sharp.Remote r in remoteCollection.Where(r => !string.IsNullOrWhiteSpace(r.Name) && !string.IsNullOrWhiteSpace(r.Url)))
                {
                    info.Remotes.Add(new Core.Plugin.Repository.Remote(r.Name.Trim(), r.Url.Trim()));
                }
            }

            return info;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not read (LibGit2Sharp) repo in {Path}.", repoPath);
            return null;
        }
    }

    private static string[] ReadAllBranches(string repoPath)
    {
        try
        {
            using var repo = new LibGit2Sharp.Repository(repoPath);
            var localBranches = repo.Branches.Where(b => !b.IsRemote).Select(b => b.FriendlyName);

            return repo.Branches
                       .Where(branch =>
                           branch.IsRemote
                           &&
                           !branch.FriendlyName.Contains("HEAD", StringComparison.OrdinalIgnoreCase))
                       .Select(branch => branch.FriendlyName.Replace("origin/", string.Empty))
                       .Except(localBranches)
                       .OrderBy(n => n)
                       .ToArray();
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static HeadDetails GetHeadDetails(LibGit2Sharp.Repository repo)
    {
        var isDetached = repo.Head.GetType().Name.EndsWith("DetachedHead", StringComparison.OrdinalIgnoreCase);

        Tag? tag = null;

        var headTipSha = repo.Head.Tip?.Sha;
        if (isDetached && headTipSha != null)
        {
            tag = repo.Tags.FirstOrDefault(t => t.Target?.Sha?.Equals(repo.Head.Tip?.Sha) ?? false);
        }

        return new HeadDetails
            {
                Name = isDetached
                    ? tag?.FriendlyName ?? headTipSha ?? repo.Head.FriendlyName
                    : repo.Head.FriendlyName,
                IsDetached = isDetached,
                IsOnTag = tag != null,
            };
    }

    private static string GetSafePath(string input)
    {
        var safePath = input.Replace('\\', '/');
        if (safePath.EndsWith('/'))
        {
            safePath = safePath[..^1];
        }

        return safePath;
    }

    private static string GetWindowsPath(string input)
    {
        var safePath = input.Replace('/', '\\');
        if (safePath.EndsWith('\\'))
        {
            safePath = safePath[..^1];
        }

        return safePath;
    }

    private readonly record struct HeadDetails
    {
        public HeadDetails()
        {
        }

        internal required string Name { get; init; }

        internal required bool IsDetached { get; init; }

        internal required bool IsOnTag { get; init; }
    }
}
