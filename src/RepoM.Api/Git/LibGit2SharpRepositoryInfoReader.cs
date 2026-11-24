namespace RepoM.Api.Git;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using RepoM.Api.IO.ModuleBasedRepositoryActionProvider;
using CliCommandException = RepoM.Api.Git.ProcessExecution.GitCommandException;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Reading;

[ExcludeFromCodeCoverage]
public class LibGit2SharpRepositoryInfoReader : IRepositoryInfoReader
{
    private readonly IRepositoryTagsFactory _resolver;
    private readonly ILogger _logger;
    private readonly IGitCommander _gitCommander;

    public LibGit2SharpRepositoryInfoReader(IRepositoryTagsFactory resolver, IGitCommander gitCommander, ILogger logger)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gitCommander = gitCommander ?? throw new ArgumentNullException(nameof(gitCommander));
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
                info = await ReadInternalAsync(repoPath, readId, path, currentTry, ct).ConfigureAwait(false);
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

    private async Task<RepositoryInfo?> ReadInternalAsync(string repoPath, Guid readId, string path, int attempt, CancellationToken ct)
    {
        try
        {
            using var repo = ReadStage(() => new LibGit2Sharp.Repository(repoPath), readId, path, "open", attempt, ct);
            ct.ThrowIfCancellationRequested();

            WorkingTreeStatus? status = null;
            var workingDirectory = new DirectoryInfo(repoPath);

            if (!repo.Info.IsBare)
            {
                workingDirectory = new DirectoryInfo(repo.Info.WorkingDirectory);
                var statusTimer = Stopwatch.StartNew();
                var statusOutcome = "failed";
                try
                {
                    var output = await _gitCommander.CommandAsync(workingDirectory.FullName, ct,
                        "status", "--porcelain=v2", "--branch", "--ahead-behind", "-z", "--untracked-files=all", "--ignored=no",
                        "--no-renames", "--ignore-submodules=none").ConfigureAwait(false);
                    status = WorkingTreeStatus.Parse(output, ct);
                    statusOutcome = "completed";
                }
                finally
                {
                    LogReadStage(readId, path, "status", attempt, statusTimer,
                        ct.IsCancellationRequested ? "cancelled" : statusOutcome);
                }

                ct.ThrowIfCancellationRequested();
            }

            if (string.IsNullOrWhiteSpace(workingDirectory.Parent?.FullName))
            {
                _logger.LogError("WorkingDirectory.Parent.Fullname was null or empty for repository found in '{Path}'. Return null", repoPath);
                return null;
            }

            HeadDetails headDetails = status is { BranchName: not "(detached)" } headStatus
                ? new HeadDetails { Name = headStatus.BranchName, IsDetached = false, IsOnTag = false }
                : ReadStage(() => GetHeadDetails(repo, status), readId, path, "head", attempt, ct);

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
        catch (CliCommandException e)
        {
            ct.ThrowIfCancellationRequested();
            _logger.LogWarning(e, "Could not read Git status for repository {Path}", repoPath);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not read (LibGit2Sharp) repo in {Path}.", repoPath);
            return null;
        }
    }

    private static RepositoryInfo ReadMetadata(LibGit2Sharp.Repository repo, string repoPath, WorkingTreeStatus? status,
        DirectoryInfo workingDirectory, HeadDetails headDetails, string[] allBranchNames, string[] localBranchNames, CancellationToken ct)
    {
        var fullPath = workingDirectory.FullName;
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
            CurrentBranchHasUpstream = status is { } branchStatus
                ? branchStatus.Upstream != null
                : !string.IsNullOrEmpty(repo.Head.UpstreamBranchCanonicalName),
            CurrentBranchIsDetached = headDetails.IsDetached,
            CurrentBranchIsOnTag = headDetails.IsOnTag,
            AheadBy = status is { } aheadStatus ? aheadStatus.AheadBy : repo.Head.TrackingDetails?.AheadBy,
            BehindBy = status is { } behindStatus ? behindStatus.BehindBy : repo.Head.TrackingDetails?.BehindBy,
            LocalUntracked = status?.Untracked,
            LocalModified = status?.Modified,
            LocalMissing = status?.Missing,
            LocalAdded = status?.Added,
            LocalStaged = status?.Staged,
            LocalRemoved = status?.Removed,
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
            "Repository read {ReadId} for {Path}: stage={Stage}; attempt={Attempt}; elapsed={ElapsedMs:F1} ms; outcome={Outcome}; backend={Backend}",
            readId, path, stage, attempt, timer.Elapsed.TotalMilliseconds, outcome,
            stage == "status" ? "git-cli" : stage is "discover" or "open" or "head" or "branches" or "metadata" ? "libgit2sharp" : "repom");
    }

    private readonly record struct WorkingTreeStatus(int Untracked, int Modified, int Missing, int Added, int Staged, int Removed)
    {
        public string BranchName { get; init; } = string.Empty;

        public string CommitId { get; init; } = string.Empty;

        public string? Upstream { get; init; }

        public int? AheadBy { get; init; }

        public int? BehindBy { get; init; }

        public static WorkingTreeStatus Parse(string output, CancellationToken ct)
        {
            var counts = new WorkingTreeStatus();
            string? branch = null;
            string? oid = null;
            string? upstream = null;
            int? ahead = null;
            int? behind = null;
            var remaining = output.AsSpan();
            while (!remaining.IsEmpty)
            {
                ct.ThrowIfCancellationRequested();
                var end = remaining.IndexOf('\0');
                if (end < 3 || remaining[1] != ' ')
                {
                    throw new CliCommandException("Invalid NUL-delimited Git porcelain v2 status output.");
                }

                var record = remaining[..end];
                remaining = remaining[(end + 1)..];
                if (record[0] == '#')
                {
                    if (record.StartsWith("# branch.head "))
                    {
                        if (branch != null)
                        {
                            throw new CliCommandException("Duplicate Git branch header.");
                        }

                        branch = record[14..].ToString();
                    }
                    else if (record.StartsWith("# branch.oid "))
                    {
                        if (oid != null)
                        {
                            throw new CliCommandException("Duplicate Git commit header.");
                        }

                        oid = record[13..].ToString();
                    }
                    else if (record.StartsWith("# branch.upstream "))
                    {
                        if (upstream != null)
                        {
                            throw new CliCommandException("Duplicate Git upstream header.");
                        }

                        upstream = record[18..].ToString();
                        if (upstream.Length == 0)
                        {
                            throw new CliCommandException("Empty Git upstream header.");
                        }
                    }
                    else if (record.StartsWith("# branch.ab "))
                    {
                        var values = record[12..];
                        var separator = values.IndexOf(' ');
                        if (ahead != null || separator < 2 || separator + 2 >= values.Length
                            || values[0] != '+' || values[separator + 1] != '-'
                            || !int.TryParse(values[1..separator], NumberStyles.None, CultureInfo.InvariantCulture, out var aheadValue)
                            || !int.TryParse(values[(separator + 2)..], NumberStyles.None, CultureInfo.InvariantCulture, out var behindValue))
                        {
                            throw new CliCommandException("Invalid Git ahead/behind header.");
                        }

                        ahead = aheadValue;
                        behind = behindValue;
                    }

                    // Porcelain v2 permits additional headers; ignore unknown header names.
                    continue;
                }

                if (record[0] == '?')
                {
                    counts = counts with { Untracked = counts.Untracked + 1 };
                    continue;
                }

                var fields = record.ToString().Split(' ', record[0] == 'u' ? 11 : 9);
                if (record[0] == 'u' && fields.Length == 11 && fields[10].Length > 0
                    && fields[1] is "DD" or "AU" or "UD" or "UA" or "DU" or "AA" or "UU")
                {
                    // LibGit2Sharp's six existing counters do not include conflicted entries.
                    continue;
                }

                if (record[0] != '1' || fields.Length != 9 || fields[8].Length == 0 || fields[1].Length != 2)
                {
                    throw new CliCommandException("Unexpected Git porcelain v2 record (rename detection must be disabled).");
                }

                var code = fields[1];
                if (!".MADT".Contains(code[0]) || !".MDT".Contains(code[1]) || code == "..")
                {
                    throw new CliCommandException("Unexpected Git status code.");
                }

                counts = counts with
                {
                    Added = counts.Added + (code[0] == 'A' ? 1 : 0),
                    Staged = counts.Staged + (code[0] == 'M' ? 1 : 0),
                    Removed = counts.Removed + (code[0] == 'D' ? 1 : 0),
                    Modified = counts.Modified + (code[1] == 'M' ? 1 : 0),
                    Missing = counts.Missing + (code[1] == 'D' ? 1 : 0),
                };
            }

            if (string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(oid)
                || (oid != "(initial)" && (oid.Length is not (40 or 64) || oid.Any(c => !Uri.IsHexDigit(c))))
                || (branch == "(detached)" && (oid == "(initial)" || upstream != null))
                || (ahead != null && (upstream == null || oid == "(initial)")))
            {
                throw new CliCommandException("Missing or inconsistent Git branch headers.");
            }

            return counts with { BranchName = branch, CommitId = oid, Upstream = upstream, AheadBy = ahead, BehindBy = behind };
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

    private static HeadDetails GetHeadDetails(LibGit2Sharp.Repository repo, WorkingTreeStatus? status)
    {
        var isDetached = status is { } branchStatus
            ? branchStatus.BranchName == "(detached)"
            : repo.Head.GetType().Name.EndsWith("DetachedHead", StringComparison.OrdinalIgnoreCase);

        Tag? tag = null;

        var headTipSha = status is { } commitStatus ? commitStatus.CommitId : repo.Head.Tip?.Sha;
        if (isDetached && headTipSha != null)
        {
            tag = repo.Tags.FirstOrDefault(t => t.Target?.Sha?.Equals(headTipSha) ?? false);
        }

        return new HeadDetails
        {
            Name = isDetached
                    ? tag?.FriendlyName ?? headTipSha ?? repo.Head.FriendlyName
                    : status is { } nameStatus ? nameStatus.BranchName : repo.Head.FriendlyName,
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