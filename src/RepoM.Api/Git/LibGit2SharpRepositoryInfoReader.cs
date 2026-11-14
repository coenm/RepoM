namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Reading;

[ExcludeFromCodeCoverage]
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
        ct.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var readId = Guid.NewGuid();
        var timer = Stopwatch.StartNew();
        var outcome = "failed";
        try
        {
            var repoPath = ReadStage(() => LibGit2Sharp.Repository.Discover(path), readId, path, "discover", 0, ct);
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(repoPath))
            {
                _logger.LogWarning("Could not Discover git repo in path {Path}", path);
                return null;
            }

            RepositoryInfo? result = await ReadWithRetries(repoPath, 3, readId, path, ct).ConfigureAwait(false);
            ct.ThrowIfCancellationRequested();
            if (result != null)
            {
                var tagsTimer = Stopwatch.StartNew();
                var tagsOutcome = "failed";
                try
                {
                    var tempAdapter = new RepoM.Core.Repositories.Adapters.RepositoryInfoAdapter(result);
                    result.Tags = (await _resolver.GetTagsAsync(tempAdapter).ConfigureAwait(false)).ToArray();
                    ct.ThrowIfCancellationRequested();
                    tagsOutcome = "completed";
                }
                finally
                {
                    LogReadStage(readId, path, "repom-tags", 0, tagsTimer,
                        ct.IsCancellationRequested ? "cancelled" : tagsOutcome);
                }

                outcome = "completed";
            }
            else
            {
                _logger.LogWarning("Could not read git repo in path {Path}", repoPath);
            }

            return result;
        }
        finally
        {
            LogReadStage(readId, path, "total", 0, timer, ct.IsCancellationRequested ? "cancelled" : outcome);
        }
    }

    private async Task<RepositoryInfo?> ReadWithRetries(string repoPath, int maxRetries, Guid readId, string path, CancellationToken ct)
    {
        RepositoryInfo? info = null;
        var currentTry = 1;

        while (info == null && currentTry <= maxRetries)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                info = ReadInternal(repoPath, readId, path, currentTry, ct);
            }
            catch (LockedFileException e)
            {
                _logger.LogWarning(e, "LockedFileException {Path}", repoPath);

                if (currentTry >= maxRetries)
                {
                    throw;
                }

                var timer = Stopwatch.StartNew();
                try
                {
                    await Task.Delay(500, ct).ConfigureAwait(false);
                }
                finally
                {
                    LogReadStage(readId, path, "retry-delay", currentTry, timer,
                        ct.IsCancellationRequested ? "cancelled" : "completed");
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
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

    private RepositoryInfo? ReadInternal(string repoPath, Guid readId, string path, int attempt, CancellationToken ct)
    {
        try
        {
            using var repo = ReadStage(() => new LibGit2Sharp.Repository(repoPath), readId, path, "open", attempt, ct);
            ct.ThrowIfCancellationRequested();

            RepositoryStatus? status = null;
            var workingDirectory = new DirectoryInfo(repoPath);

            if (!repo.Info.IsBare)
            {
                status = ReadStage(() => repo.RetrieveStatus(new StatusOptions
                {
                    IncludeIgnored = false,
                    DetectRenamesInIndex = false,
                    DetectRenamesInWorkDir = false,
                }), readId, path, "status", attempt, ct);
                ct.ThrowIfCancellationRequested();
                workingDirectory = new DirectoryInfo(repo.Info.WorkingDirectory);
            }

            if (string.IsNullOrWhiteSpace(workingDirectory.Parent?.FullName))
            {
                _logger.LogError("WorkingDirectory.Parent.Fullname was null or empty for repository found in '{Path}'. Return null", repoPath);
                return null;
            }

            HeadDetails headDetails = ReadStage(() => GetHeadDetails(repo), readId, path, "head", attempt, ct);

            var (allBranchNames, localBranchNames) = ReadStage(() =>
            {
                var allBranchList = new List<string>();
                var localBranchList = new List<string>();
                foreach (Branch branch in repo.Branches)
                {
                    ct.ThrowIfCancellationRequested();
                    allBranchList.Add(branch.FriendlyName);
                    if (!branch.IsRemote)
                    {
                        localBranchList.Add(branch.FriendlyName);
                    }
                }

                return (allBranchList.ToArray(), localBranchList.ToArray());
            }, readId, path, "branches", attempt, ct);

            return ReadStage(() => ReadMetadata(repo, repoPath, status, workingDirectory, headDetails,
                allBranchNames, localBranchNames, ct), readId, path, "metadata", attempt, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (LockedFileException)
        {
            // Leave lock retries (and their cancellation/delay) to ReadWithRetries.
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not read (LibGit2Sharp) repo in {Path}.", repoPath);
            return null;
        }
    }

    private static RepositoryInfo ReadMetadata(LibGit2Sharp.Repository repo, string repoPath, RepositoryStatus? status,
        DirectoryInfo workingDirectory, HeadDetails headDetails, string[] allBranchNames, string[] localBranchNames, CancellationToken ct)
    {
        var fullPath = workingDirectory.FullName;
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
        ct.ThrowIfCancellationRequested();
        if (remoteCollection != null)
        {
            foreach (LibGit2Sharp.Remote r in remoteCollection.Where(r => !string.IsNullOrWhiteSpace(r.Name) && !string.IsNullOrWhiteSpace(r.Url)))
            {
                ct.ThrowIfCancellationRequested();
                info.Remotes.Add(new Core.Plugin.Repository.Remote(r.Name.Trim(), r.Url.Trim()));
            }
        }

        return info;
    }

    private T ReadStage<T>(Func<T> read, Guid readId, string path, string stage, int attempt, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var timer = Stopwatch.StartNew();
        var outcome = "failed";
        try
        {
            var result = read();
            outcome = "completed";
            return result;
        }
        finally
        {
            LogReadStage(readId, path, stage, attempt, timer, ct.IsCancellationRequested ? "cancelled" : outcome);
        }
    }

    private void LogReadStage(Guid readId, string path, string stage, int attempt, Stopwatch timer, string outcome)
    {
        _logger.LogInformation(
            "Repository read {ReadId} for {Path}: stage={Stage}; attempt={Attempt}; elapsed={ElapsedMs:F1} ms; outcome={Outcome}",
            readId, path, stage, attempt, timer.Elapsed.TotalMilliseconds, outcome);
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