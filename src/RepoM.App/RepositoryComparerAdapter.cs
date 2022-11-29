namespace RepoM.App;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Core.Plugin.RepositoryOrdering;

public interface IRepositoryComparerManager
{
    event EventHandler<string>? SelectedRepositoryComparerKeyChanged;

    IComparer Comparer { get; }

    IReadOnlyList<string> RepositoryComparerKeys { get; }

    bool SetRepositoryComparer(string key);
}

internal class RepositoryComparerManager : IRepositoryComparerManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ComparerComposition _comparer;
    private readonly List<string> _repositoryComparerKeys;

    public RepositoryComparerManager(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _comparer = new ComparerComposition(new Dictionary<string, IComparer>());
        SelectedRepositoryComparerKey = string.Empty;
        _repositoryComparerKeys = new List<string>();
    }

    public event EventHandler<string>? SelectedRepositoryComparerKeyChanged;

    public IComparer Comparer => _comparer;

    public string SelectedRepositoryComparerKey { get; private set; }

    public IReadOnlyList<string> RepositoryComparerKeys => _repositoryComparerKeys;

    public bool SetRepositoryComparer(string key)
    {
        if (_comparer.SetComparer(key))
        {
            SelectedRepositoryComparerKey = key;
            SelectedRepositoryComparerKeyChanged?.Invoke(this, key);
            return true;
        }

        return false;
    }
}

internal class ComparerComposition : IComparer
{
    private readonly Dictionary<string, IComparer> _namedComparers;
    private IComparer _selected;

    public ComparerComposition(Dictionary<string, IComparer> namedNamedComparers)
    {
        _namedComparers = namedNamedComparers;
        _selected = _namedComparers.First().Value;
    }

    public bool SetComparer(string key)
    {
        if (_namedComparers.TryGetValue(key, out IComparer? value))
        {
            _selected = value;
            return true;
        }

        return false;
    }

    public int Compare(object? x, object? y)
    {
        IComparer comparer = _selected;
        return comparer.Compare(x, y);
    }
}

internal class RepositoryComparerAdapter : IComparer
{
    private readonly IRepositoryComparer _comparer;

    public RepositoryComparerAdapter(IRepositoryComparer comparer)
    {
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    public int Compare(object? x, object? y)
    {
        if (x is IRepositoryView xView && y is IRepositoryView yView)
        {
            return Compare(xView, yView);
        }

        return 0;
    }

    private int Compare(IRepositoryView x, IRepositoryView y)
    {
        return _comparer.Compare(x.Repository, y.Repository);
    }
}