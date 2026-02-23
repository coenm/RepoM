namespace RepoM.Api.Git;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Pinning;

[DebuggerDisplay("{Name} @{Path}")]
public class RepositoryViewModel : IRepositoryView, INotifyPropertyChanged
{
    private readonly IPinningService _pinningService;
    private readonly RepositoryInfo _info;
    private readonly RepositoryInfoAdapter _adapter;
    private string? _cachedRepositoryStatusCode;
    private string? _cachedRepositoryStatus;
    private string? _cachedRepositoryStatusWithBranch;
    private bool _isSynchronizing;

    public event PropertyChangedEventHandler? PropertyChanged;

    public RepositoryViewModel(RepositoryInfo info, IPinningService pinningService)
    {
        _info = info ?? throw new ArgumentNullException(nameof(info));
        _pinningService = pinningService ?? throw new ArgumentNullException(nameof(pinningService));
        _adapter = new RepositoryInfoAdapter(info);
        Tags = _info.Tags.Select(tag => new TagViewModel(tag)).ToArray();
    }

    public override bool Equals(object? obj)
    {
        if (obj is RepositoryViewModel other)
        {
            return string.Equals(other._info.SafePath, _info.SafePath, StringComparison.OrdinalIgnoreCase);
        }

        return ReferenceEquals(this, obj);
    }

    private void EnsureStatusCache()
    {
        var repositoryStatusCode = _info.GetStatusCode();

        // compare the status code and not the full status string because the latter one is heavier to calculate
        var canTakeFromCache = string.Equals(_cachedRepositoryStatusCode, repositoryStatusCode, StringComparison.CurrentCulture);

        if (canTakeFromCache)
        {
            return;
        }

        _cachedRepositoryStatus = StatusCompressor.Compress(_info);
        _cachedRepositoryStatusWithBranch = StatusCompressor.CompressWithBranch(_info);
        _cachedRepositoryStatusCode = repositoryStatusCode;
    }

    public bool IsPinned => _pinningService.IsPinned(_info.SafePath);

    public bool IsNotBare => !_info.IsBare;

    public string Name => _info.Name + (IsSynchronizing ? SyncAppendix : string.Empty);

    public string Path => _info.Path;

    public string Location => _info.Location;

    public string CurrentBranch => _info.CurrentBranch;

    public string AheadBy => _info.AheadBy?.ToString() ?? string.Empty;

    public string BehindBy => _info.BehindBy?.ToString() ?? string.Empty;

    public string[] Branches => _info.Branches ?? [];

    public string LocalUntracked => _info.LocalUntracked?.ToString() ?? string.Empty;

    public string LocalModified => _info.LocalModified?.ToString() ?? string.Empty;

    public string LocalMissing => _info.LocalMissing?.ToString() ?? string.Empty;

    public string LocalAdded => _info.LocalAdded?.ToString() ?? string.Empty;

    public string LocalStaged => _info.LocalStaged?.ToString() ?? string.Empty;

    public string LocalRemoved => _info.LocalRemoved?.ToString() ?? string.Empty;

    public string LocalIgnored => _info.LocalIgnored?.ToString() ?? string.Empty;

    public string StashCount => _info.StashCount?.ToString() ?? string.Empty;

    public bool WasFound => _info.WasFound;

    public bool HasUnpushedChanges => _info.HasUnpushedChanges;

    public TagViewModel[] Tags { get; }

    public override int GetHashCode()
    {
        return _info.SafePath.GetHashCode();
    }

    public IRepository Repository => _adapter;

    public RepositoryInfo RepositoryInfo => _info;

    public string Status
    {
        get
        {
            EnsureStatusCache();
            return _cachedRepositoryStatus!;
        }
    }

    public string BranchWithStatus
    {
        get
        {
            EnsureStatusCache();
            return _cachedRepositoryStatusWithBranch!;
        }
    }

    public bool IsSynchronizing
    {
        get => _isSynchronizing;
        set
        {
            _isSynchronizing = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); // Name includes the activity icon
        }
    }

    private static string SyncAppendix => "  \u2191\u2193"; // up and down arrows
}
