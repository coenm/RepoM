namespace RepoM.Core.Repositories.Persistence;

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Model;

public sealed class RepositorySnapshotStore : IRepositorySnapshotStore
{
    private const int CURRENT_VERSION = 1;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly string _filePath;

    public RepositorySnapshotStore(IFileSystem fileSystem, RepositorySnapshotStoreSettings settings, ILogger logger)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        ArgumentNullException.ThrowIfNull(settings);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _filePath = settings.FilePath;
    }

    public async Task<IReadOnlyList<RepositoryInfo>> LoadAsync(CancellationToken ct = default)
    {
        if (!_fileSystem.File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            var json = await _fileSystem.File.ReadAllTextAsync(_filePath, ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            SnapshotDocument? document = JsonSerializer.Deserialize<SnapshotDocument>(json, _jsonOptions);
            if (document?.Repositories is null || document.Version != CURRENT_VERSION)
            {
                return [];
            }

            return [.. document.Repositories.Select(Map),];
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not load repository snapshot from '{File}'", _filePath);
            return [];
        }
    }

    public async Task SaveAsync(IEnumerable<RepositoryInfo> repositories, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(repositories);

        try
        {
            var directory = _fileSystem.Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !_fileSystem.Directory.Exists(directory))
            {
                _fileSystem.Directory.CreateDirectory(directory);
            }

            var document = new SnapshotDocument
            {
                Version = CURRENT_VERSION,
                Repositories = [.. repositories.Select(Map),],
            };

            var json = JsonSerializer.Serialize(document, _jsonOptions);

            // Write to a temporary file first, then replace, to avoid a corrupted snapshot on crash.
            var tempFile = _filePath + ".tmp";
            await _fileSystem.File.WriteAllTextAsync(tempFile, json, ct).ConfigureAwait(false);
            _fileSystem.File.Move(tempFile, _filePath, overwrite: true);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not save repository snapshot to '{File}'", _filePath);
        }
    }

    private static RepositorySnapshotDto Map(RepositoryInfo info)
    {
        return new RepositorySnapshotDto
        {
            Path = info.Path,
            SafePath = info.SafePath,
            Name = info.Name,
            WindowsPath = info.WindowsPath,
            LinuxPath = info.LinuxPath,
            Location = info.Location,
            IsBare = info.IsBare,
            CurrentBranch = info.CurrentBranch,
            CurrentBranchHasUpstream = info.CurrentBranchHasUpstream,
            CurrentBranchIsDetached = info.CurrentBranchIsDetached,
            CurrentBranchIsOnTag = info.CurrentBranchIsOnTag,
            Branches = info.Branches,
            LocalBranches = info.LocalBranches,
            Tags = info.Tags,
            AheadBy = info.AheadBy,
            BehindBy = info.BehindBy,
            LocalUntracked = info.LocalUntracked,
            LocalModified = info.LocalModified,
            LocalMissing = info.LocalMissing,
            LocalAdded = info.LocalAdded,
            LocalStaged = info.LocalStaged,
            LocalRemoved = info.LocalRemoved,
            StashCount = info.StashCount,
            LastSeen = info.LastSeen,
            LastUpdated = info.LastUpdated,
            Remotes =
                [
                    .. info.Remotes.Select(r => new RemoteDto
                        {
                            Key = r.Key,
                            Url = r.Url,
                        }),
                ],
        };
    }

    private static RepositoryInfo Map(RepositorySnapshotDto dto)
    {
        var info = new RepositoryInfo
        {
            Path = dto.Path,
            SafePath = dto.SafePath,
            Name = dto.Name,
            WindowsPath = dto.WindowsPath,
            LinuxPath = dto.LinuxPath,
            Location = dto.Location,
            IsBare = dto.IsBare,
            CurrentBranch = dto.CurrentBranch,
            CurrentBranchHasUpstream = dto.CurrentBranchHasUpstream,
            CurrentBranchIsDetached = dto.CurrentBranchIsDetached,
            CurrentBranchIsOnTag = dto.CurrentBranchIsOnTag,
            Branches = dto.Branches,
            LocalBranches = dto.LocalBranches,
            Tags = dto.Tags,
            AheadBy = dto.AheadBy,
            BehindBy = dto.BehindBy,
            LocalUntracked = dto.LocalUntracked,
            LocalModified = dto.LocalModified,
            LocalMissing = dto.LocalMissing,
            LocalAdded = dto.LocalAdded,
            LocalStaged = dto.LocalStaged,
            LocalRemoved = dto.LocalRemoved,
            StashCount = dto.StashCount,
            LastSeen = dto.LastSeen,
            LastUpdated = dto.LastUpdated,
        };

        foreach (RemoteDto remote in dto.Remotes)
        {
            info.Remotes.Add(new Remote(remote.Key, remote.Url));
        }

        return info;
    }

    private sealed class SnapshotDocument
    {
        public int Version { get; set; }

        public List<RepositorySnapshotDto> Repositories { get; set; } = [];
    }

    private sealed class RepositorySnapshotDto
    {
        public string Path { get; set; } = string.Empty;

        public string SafePath { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string WindowsPath { get; set; } = string.Empty;

        public string LinuxPath { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public bool IsBare { get; set; }

        public string CurrentBranch { get; set; } = string.Empty;

        public bool CurrentBranchHasUpstream { get; set; }

        public bool CurrentBranchIsDetached { get; set; }

        public bool CurrentBranchIsOnTag { get; set; }

        public string[] Branches { get; set; } = [];

        public string[] LocalBranches { get; set; } = [];

        public string[] Tags { get; set; } = [];

        public int? AheadBy { get; set; }

        public int? BehindBy { get; set; }

        public int? LocalUntracked { get; set; }

        public int? LocalModified { get; set; }

        public int? LocalMissing { get; set; }

        public int? LocalAdded { get; set; }

        public int? LocalStaged { get; set; }

        public int? LocalRemoved { get; set; }

        public int? StashCount { get; set; }

        public DateTimeOffset LastSeen { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public List<RemoteDto> Remotes { get; set; } = [];
    }

    private sealed class RemoteDto
    {
        public string Key { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;
    }
}
