namespace RepoM.App.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

public sealed class WorktreePreviewViewModel : INotifyPropertyChanged
{
    private string _filter = string.Empty;
    private bool _enableAnimations = true;

    public WorktreePreviewViewModel()
    {
        // XamlWriter needs public collection types for bound tag lists, not compiler-generated read-only arrays.
        Families =
        [
            new(new("RepoM", "main", @"C:\dev\RepoM", "\u21910 \u21930  |  ~2", isFavorite: true) { Tags = new[] { "github", "csharp", "work" } },
            [
                new("feature-login", "feature/login", @"C:\dev\RepoM.worktrees\feature-login", "\u21912 \u21930  |  +1 ~3", isWorktree: true) { Tags = new[] { "csharp", "authentication" } },
                new("hotfix", "hotfix/crash", @"C:\dev\RepoM.worktrees\hotfix", "\u21910 \u21931", isWorktree: true, isFavorite: true) { Tags = new[] { "csharp", "urgent" } },
            ]),
            new(new("Payments.Api", "develop", @"C:\dev\Payments.Api", "\u21910 \u21930") { Tags = new[] { "work", "backend" } },
            [
                new("retry-policy", "feature/retry-policy", @"C:\dev\Payments.worktrees\retry-policy", "\u21911 \u21930  |  ~4", isWorktree: true) { Tags = new[] { "backend", "resilience" } },
                new("release", "release/2.4", @"C:\dev\Payments.worktrees\release", "\u21910 \u21930", isWorktree: true) { Tags = new[] { "backend", "release" } },
            ]),
            new(new("Toolbox", "main", @"C:\dev\Toolbox", "\u21910 \u21930") { Tags = new[] { "personal", "tools" } }, []),
        ];

        foreach (WorktreePreviewFamily family in Families)
        {
            family.Primary.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(WorktreePreviewEntry.DisplayMode))
                {
                    Refresh();
                }
            };
        }

        Refresh();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<WorktreePreviewFamily> Families { get; }

    public ObservableCollection<WorktreePreviewEntry> VisibleEntries { get; } = [];

    public bool IsEmpty => VisibleEntries.Count == 0;

    public bool EnableAnimations
    {
        get => _enableAnimations;
        set
        {
            if (_enableAnimations == value)
            {
                return;
            }

            _enableAnimations = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnableAnimations)));
        }
    }

    public string Filter
    {
        get => _filter;
        set
        {
            if (_filter == value)
            {
                return;
            }

            _filter = value;
            Refresh();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filter)));
        }
    }

    private void Refresh()
    {
        var query = Filter.Trim();
        var isFiltering = query.Length > 0;
        var entries = new List<WorktreePreviewEntry>();
        foreach (WorktreePreviewFamily family in Families)
        {
            WorktreePreviewEntry[] children = family.Worktrees.Where(entry => entry.Matches(query)).ToArray();
            var primaryMatches = family.Primary.Matches(query);
            family.Primary.SetFilterState(isFiltering, !primaryMatches && children.Length > 0, children.Length > 0);
            foreach (WorktreePreviewEntry worktree in family.Worktrees)
            {
                worktree.SetFilterState(isFiltering, false, false);
            }

            if (!primaryMatches && children.Length == 0)
            {
                continue;
            }

            entries.Add(family.Primary);
            if (family.Primary.AreWorktreesVisible)
            {
                entries.AddRange(children);
            }
        }

        // Keep surviving rows selected and focused when their children are shown or hidden.
        for (var index = VisibleEntries.Count - 1; index >= 0; index--)
        {
            if (!entries.Contains(VisibleEntries[index]))
            {
                VisibleEntries.RemoveAt(index);
            }
        }

        for (var index = 0; index < entries.Count; index++)
        {
            if (index >= VisibleEntries.Count || VisibleEntries[index] != entries[index])
            {
                VisibleEntries.Insert(index, entries[index]);
            }
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEmpty)));
    }
}

public sealed class WorktreePreviewFamily
{
    public WorktreePreviewFamily(WorktreePreviewEntry primary, IReadOnlyList<WorktreePreviewEntry> worktrees)
    {
        Primary = primary;
        Worktrees = worktrees;
        Primary.WorktreeCount = worktrees.Count;
        Primary.DisplayMode = worktrees.Count > 0
            ? WorktreePreviewDisplayMode.ExtendedWithWorktrees
            : WorktreePreviewDisplayMode.Extended;
    }

    public WorktreePreviewEntry Primary { get; }

    public IReadOnlyList<WorktreePreviewEntry> Worktrees { get; }
}

public enum WorktreePreviewDisplayMode
{
    Minimal,
    Extended,
    ExtendedWithWorktrees,
}

public sealed class WorktreePreviewEntry : INotifyPropertyChanged
{
    private WorktreePreviewDisplayMode _displayMode = WorktreePreviewDisplayMode.Extended;
    private bool _isFavorite;
    private bool _isFiltering;
    private bool _hasMatchingChildren;
    private bool _isContextOnly;

    public WorktreePreviewEntry(string name, string branch, string path, string status, bool isWorktree = false, bool isFavorite = false)
    {
        Name = name;
        Branch = branch;
        Path = path;
        Status = status;
        IsWorktree = isWorktree;
        _isFavorite = isFavorite;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; }

    public string Branch { get; }

    public string Path { get; }

    public string Status { get; }

    public IReadOnlyList<string> Tags { get; init; } = [];

    public bool IsWorktree { get; }

    public int WorktreeCount { get; internal set; }

    public bool HasWorktrees => WorktreeCount > 0;

    public bool IsContextOnly => _isContextOnly;

    public bool CanChangeDisplayMode => !_isFiltering;

    public WorktreePreviewDisplayMode DisplayedMode => _isFiltering
        ? HasWorktrees && _hasMatchingChildren
            ? WorktreePreviewDisplayMode.ExtendedWithWorktrees
            : WorktreePreviewDisplayMode.Extended
        : DisplayMode;

    public bool AreDetailsVisible => DisplayedMode != WorktreePreviewDisplayMode.Minimal;

    public bool AreWorktreesVisible => DisplayedMode == WorktreePreviewDisplayMode.ExtendedWithWorktrees;

    public WorktreePreviewDisplayMode DisplayMode
    {
        get => _displayMode;
        set
        {
            if (!Enum.IsDefined(value) || (value == WorktreePreviewDisplayMode.ExtendedWithWorktrees && !HasWorktrees))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The display mode must be supported by this entry.");
            }

            if (_displayMode == value)
            {
                return;
            }

            _displayMode = value;
            OnPropertyChanged();
            NotifyDisplayedMode();
        }
    }

    public void CycleDisplayMode()
    {
        if (!CanChangeDisplayMode)
        {
            return;
        }

        DisplayMode = DisplayMode switch
        {
            WorktreePreviewDisplayMode.Minimal => WorktreePreviewDisplayMode.Extended,
            WorktreePreviewDisplayMode.Extended when HasWorktrees => WorktreePreviewDisplayMode.ExtendedWithWorktrees,
            _ => WorktreePreviewDisplayMode.Minimal,
        };
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite == value)
            {
                return;
            }

            _isFavorite = value;
            OnPropertyChanged();
        }
    }

    internal bool Matches(string query)
    {
        return Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Branch.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Path.Contains(query, StringComparison.OrdinalIgnoreCase)
            || Tags.Any(tag => tag.Equals(query, StringComparison.OrdinalIgnoreCase));
    }

    internal void SetFilterState(bool isFiltering, bool isContextOnly, bool hasMatchingChildren)
    {
        _isFiltering = isFiltering;
        _isContextOnly = isContextOnly;
        _hasMatchingChildren = hasMatchingChildren;
        OnPropertyChanged(nameof(IsContextOnly));
        OnPropertyChanged(nameof(CanChangeDisplayMode));
        NotifyDisplayedMode();
    }

    private void NotifyDisplayedMode()
    {
        OnPropertyChanged(nameof(DisplayedMode));
        OnPropertyChanged(nameof(AreDetailsVisible));
        OnPropertyChanged(nameof(AreWorktreesVisible));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
