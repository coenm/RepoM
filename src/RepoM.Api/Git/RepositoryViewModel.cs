namespace RepoM.Api.Git;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Repositories.Adapters;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Pinning;

[DebuggerDisplay("{Name} @{Path}")]
public class RepositoryViewModel : IRepositoryView, INotifyPropertyChanged, IDisposable
{
    private static readonly PropertyChangedEventArgs _nameArgs = new(nameof(Name));
    private static readonly PropertyChangedEventArgs _currentBranchArgs = new(nameof(CurrentBranch));
    private static readonly PropertyChangedEventArgs _statusArgs = new(nameof(Status));
    private static readonly PropertyChangedEventArgs _branchWithStatusArgs = new(nameof(BranchWithStatus));
    private static readonly PropertyChangedEventArgs _aheadByArgs = new(nameof(AheadBy));
    private static readonly PropertyChangedEventArgs _behindByArgs = new(nameof(BehindBy));
    private static readonly PropertyChangedEventArgs _branchesArgs = new(nameof(Branches));
    private static readonly PropertyChangedEventArgs _localUntrackedArgs = new(nameof(LocalUntracked));
    private static readonly PropertyChangedEventArgs _localModifiedArgs = new(nameof(LocalModified));
    private static readonly PropertyChangedEventArgs _localMissingArgs = new(nameof(LocalMissing));
    private static readonly PropertyChangedEventArgs _localAddedArgs = new(nameof(LocalAdded));
    private static readonly PropertyChangedEventArgs _localStagedArgs = new(nameof(LocalStaged));
    private static readonly PropertyChangedEventArgs _localRemovedArgs = new(nameof(LocalRemoved));
    private static readonly PropertyChangedEventArgs _stashCountArgs = new(nameof(StashCount));
    private static readonly PropertyChangedEventArgs _hasUnpushedChangesArgs = new(nameof(HasUnpushedChanges));
    private static readonly PropertyChangedEventArgs _wasFoundArgs = new(nameof(WasFound));
    private static readonly PropertyChangedEventArgs _tagsArgs = new(nameof(Tags));
    private static readonly PropertyChangedEventArgs _isMonitoredArgs = new(nameof(IsMonitored));
    private readonly IPinningService _pinningService;
    private readonly IRepositoryMonitoringService _monitoringService;
    private readonly IRepositoryMonitoringEvents _monitoringEvents;
    private readonly RepositoryInfoAdapter _adapter;
    private RepositoryInfo _info;
    private int? _cachedRepositoryStatusCode;
    private string? _cachedRepositoryStatus;
    private string? _cachedRepositoryStatusWithBranch;
    private bool _isSynchronizing;

    public event PropertyChangedEventHandler? PropertyChanged;

    public RepositoryViewModel(RepositoryInfo info, IPinningService pinningService, IRepositoryMonitoringService monitoringService, IRepositoryMonitoringEvents monitoringEvents)
    {
        _info = info ?? throw new ArgumentNullException(nameof(info));
        _pinningService = pinningService ?? throw new ArgumentNullException(nameof(pinningService));
        _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
        _monitoringEvents = monitoringEvents ?? throw new ArgumentNullException(nameof(monitoringEvents));
        _adapter = new RepositoryInfoAdapter(info);
        Tags = _info.Tags.Select(tag => new TagViewModel(tag)).ToArray();
        _monitoringEvents.MonitoringChanged += OnMonitoringChanged;
    }

    /// <summary>
    /// Updates this view model in-place with new repository info, firing
    /// <see cref="PropertyChanged"/> only for properties that actually changed.
    /// This avoids allocating a new ViewModel and allows WPF to recycle the existing UI container.
    /// </summary>
    public void Update(RepositoryInfo newInfo)
    {
        ArgumentNullException.ThrowIfNull(newInfo);

        RepositoryInfo oldInfo = _info;
        _info = newInfo;
        _adapter.UpdateInfo(newInfo);

        // Invalidate status cache so next access recomputes
        _cachedRepositoryStatusCode = null;

        NotifyScalarProperties(oldInfo, newInfo);
        NotifyCollectionProperties(oldInfo, newInfo);
        NotifyDerivedProperties(oldInfo, newInfo);
    }

    private void NotifyScalarProperties(RepositoryInfo oldInfo, RepositoryInfo newInfo)
    {
        if (oldInfo.CurrentBranch != newInfo.CurrentBranch)
        {
            PropertyChanged?.Invoke(this, _currentBranchArgs);
            PropertyChanged?.Invoke(this, _nameArgs);
        }

        NotifyIfChanged(oldInfo.AheadBy, newInfo.AheadBy, _aheadByArgs);
        NotifyIfChanged(oldInfo.BehindBy, newInfo.BehindBy, _behindByArgs);
        NotifyIfChanged(oldInfo.LocalUntracked, newInfo.LocalUntracked, _localUntrackedArgs);
        NotifyIfChanged(oldInfo.LocalModified, newInfo.LocalModified, _localModifiedArgs);
        NotifyIfChanged(oldInfo.LocalMissing, newInfo.LocalMissing, _localMissingArgs);
        NotifyIfChanged(oldInfo.LocalAdded, newInfo.LocalAdded, _localAddedArgs);
        NotifyIfChanged(oldInfo.LocalStaged, newInfo.LocalStaged, _localStagedArgs);
        NotifyIfChanged(oldInfo.LocalRemoved, newInfo.LocalRemoved, _localRemovedArgs);
        NotifyIfChanged(oldInfo.StashCount, newInfo.StashCount, _stashCountArgs);
        NotifyIfChanged(oldInfo.HasUnpushedChanges, newInfo.HasUnpushedChanges, _hasUnpushedChangesArgs);
        NotifyIfChanged(oldInfo.WasFound, newInfo.WasFound, _wasFoundArgs);
    }

    private void NotifyCollectionProperties(RepositoryInfo oldInfo, RepositoryInfo newInfo)
    {
        if (!oldInfo.Branches.SequenceEqual(newInfo.Branches))
        {
            PropertyChanged?.Invoke(this, _branchesArgs);
        }

        if (!oldInfo.Tags.SequenceEqual(newInfo.Tags))
        {
            Tags = newInfo.Tags.Select(tag => new TagViewModel(tag)).ToArray();
            PropertyChanged?.Invoke(this, _tagsArgs);
        }
    }

    private void NotifyDerivedProperties(RepositoryInfo oldInfo, RepositoryInfo newInfo)
    {
        // Status depends on multiple fields; always notify when the status code changed.
        if (oldInfo.GetStatusCode() != newInfo.GetStatusCode())
        {
            PropertyChanged?.Invoke(this, _statusArgs);
            PropertyChanged?.Invoke(this, _branchWithStatusArgs);
        }
    }

    private void NotifyIfChanged<T>(T oldValue, T newValue, PropertyChangedEventArgs args)
    {
        if (!Equals(oldValue, newValue))
        {
            PropertyChanged?.Invoke(this, args);
        }
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

        if (_cachedRepositoryStatusCode == repositoryStatusCode)
        {
            return;
        }

        _cachedRepositoryStatus = StatusCompressor.Compress(_info);
        _cachedRepositoryStatusWithBranch = StatusCompressor.CompressWithBranch(_info);
        _cachedRepositoryStatusCode = repositoryStatusCode;
    }

    public bool IsPinned => _pinningService.IsPinned(_info.SafePath);

    public bool IsMonitored => _monitoringService.IsMonitored(_info.SafePath);

    public void ToggleMonitoring()
    {
        _monitoringService.SetMonitored(_info.SafePath, !IsMonitored);
    }

    public void EnableMonitoring()
    {
        _monitoringService.EnableMonitoring(_info.SafePath);
    }

    public bool IsNotBare => !_info.IsBare;

    public string Name => _info.Name + (IsSynchronizing ? SyncAppendix : string.Empty);

    public string Path => _info.Path;

    public string Location => _info.Location;

    public string CurrentBranch => _info.CurrentBranch;

    public string AheadBy => NullableIntToString(_info.AheadBy);

    public string BehindBy => NullableIntToString(_info.BehindBy);

    public string[] Branches => _info.Branches ?? [];

    public string LocalUntracked => NullableIntToString(_info.LocalUntracked);

    public string LocalModified => NullableIntToString(_info.LocalModified);

    public string LocalMissing => NullableIntToString(_info.LocalMissing);

    public string LocalAdded => NullableIntToString(_info.LocalAdded);

    public string LocalStaged => NullableIntToString(_info.LocalStaged);

    public string LocalRemoved => NullableIntToString(_info.LocalRemoved);

    public string StashCount => NullableIntToString(_info.StashCount);

    public bool WasFound => _info.WasFound;

    public bool HasUnpushedChanges => _info.HasUnpushedChanges;

    public TagViewModel[] Tags { get; private set; }

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
            PropertyChanged?.Invoke(this, _nameArgs); // Name includes the activity icon
        }
    }

    private static readonly string[] _smallIntStrings = CreateSmallIntStrings();

    private static string[] CreateSmallIntStrings()
    {
        var strings = new string[100];
        for (var i = 0; i < strings.Length; i++)
        {
            strings[i] = i.ToString();
        }

        return strings;
    }

    private static string NullableIntToString(int? value)
    {
        if (!value.HasValue)
        {
            return string.Empty;
        }

        var v = value.GetValueOrDefault();
        return (uint)v < (uint)_smallIntStrings.Length ? _smallIntStrings[v] : v.ToString();
    }

    private static string SyncAppendix => "  \u2191\u2193"; // up and down arrows

    private void OnMonitoringChanged(string safePath, bool monitored)
    {
        if (string.Equals(safePath, _info.SafePath, StringComparison.OrdinalIgnoreCase))
        {
            PropertyChanged?.Invoke(this, _isMonitoredArgs);
        }
    }

    public void Dispose()
    {
        _monitoringEvents.MonitoringChanged -= OnMonitoringChanged;
    }
}
