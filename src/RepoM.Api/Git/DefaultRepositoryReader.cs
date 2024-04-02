namespace RepoM.Api.Git;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;

public class DefaultRepositoryReader : IRepositoryReader
{
    private readonly IRepositoryTagsFactory _resolver;
    private readonly ILogger _logger;

    public DefaultRepositoryReader(IRepositoryTagsFactory resolver, ILogger logger)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Repository?> ReadRepositoryAsync(string path)
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

        Repository? result = await ReadRepositoryWithRetries(repoPath, 3).ConfigureAwait(false);
        if (result != null)
        {
            result.Tags = (await _resolver.GetTagsAsync(result).ConfigureAwait(false)).ToArray();
        }
        else
        {
            _logger.LogWarning("Could not read git repo in path {Path}", repoPath);
        }

        return result;
    }

    private async Task<Repository?> ReadRepositoryWithRetries(string repoPath, int maxRetries)
    {
        Repository? repository = null;
        var currentTry = 1;

        while (repository == null && currentTry <= maxRetries)
        {
            try
            {
                repository = ReadRepositoryInternal(repoPath);
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
                _logger.LogError(e, "Unexpected exception hwn reading repo {Path}. {Message}", repoPath, e.Message);
                throw;
            }

            currentTry++;
        }

        return repository;
    }

    private Repository? ReadRepositoryInternal(string repoPath)
    {
        try
        {
            using var repo = new LibGit2Sharp.Repository(repoPath);

            RepositoryStatus? status = null;
            var workingDirectory = new DirectoryInfo(repoPath);

            if (!repo.Info.IsBare)
            {
                status = repo.RetrieveStatus();
                workingDirectory = new DirectoryInfo(repo.Info.WorkingDirectory);
            }
            
            if (string.IsNullOrWhiteSpace(workingDirectory.Parent?.FullName))
            {
                _logger.LogError("WorkingDirectory.Parent.Fullname was null or empty for repository found in '{Path}'. Return null", repoPath);
                return null;
            }

            HeadDetails headDetails = GetHeadDetails(repo);

            var repository = new Repository(workingDirectory.FullName)
                {
                    IsBare = repo.Info.IsBare,
                    Name = workingDirectory.Name,
                    Location = workingDirectory.Parent.FullName,
                    Branches = repo.Branches.Select(b => b.FriendlyName).ToArray(),
                    LocalBranches = repo.Branches.Where(b => !b.IsRemote).Select(b => b.FriendlyName).ToArray(),
                    AllBranchesReader = () => ReadAllBranches(repoPath),
                    CurrentBranch = headDetails.Name,
                    CurrentBranchHasUpstream = !string.IsNullOrEmpty(repo.Head.UpstreamBranchCanonicalName),
                    CurrentBranchIsDetached = headDetails.IsDetached,
                    CurrentBranchIsOnTag = headDetails.IsOnTag,
                    AheadBy = repo.Head.TrackingDetails?.AheadBy,
                    BehindBy = repo.Head.TrackingDetails?.BehindBy,
                    LocalUntracked = status?.Untracked.Count(),
                    LocalModified = status?.Modified.Count(),
                    LocalMissing = status?.Missing.Count(),
                    LocalAdded = status?.Added.Count(),
                    LocalStaged = status?.Staged.Count(),
                    LocalRemoved = status?.Removed.Count(),
                    LocalIgnored = status?.Ignored.Count(),
                    StashCount = repo.Stashes?.Count() ?? 0,
                    Tags = Array.Empty<string>(),
                };

            RemoteCollection? remoteCollection = repo.Network?.Remotes;
            if (remoteCollection != null)
            {
                foreach (Remote r in remoteCollection.Where(r => !string.IsNullOrWhiteSpace(r.Name) && !string.IsNullOrWhiteSpace(r.Url)))
                {
                    repository.Remotes.Add(new Core.Plugin.Repository.Remote(r.Name.Trim(), r.Url.Trim()));
                }
            }

            return repository;
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
            var localBranches = repo.Branches.Where(b => !b.IsRemote).Select(b => b.FriendlyName).ToList();

            // "origin/" is removed from remote branches name and HEAD branch is ignored
            return repo.Branches
                       .Where(branch =>
                           branch.IsRemote
                           &&
                           !branch.FriendlyName.Contains("HEAD", StringComparison.CurrentCultureIgnoreCase))
                       .Select(branch => branch.FriendlyName.Replace("origin/", string.Empty))
                       .Except(localBranches)
                       .OrderBy(n => n)
                       .ToArray();
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }

    private static HeadDetails GetHeadDetails(LibGit2Sharp.Repository repo)
    {
        // unfortunately, type DetachedHead is internal ...
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