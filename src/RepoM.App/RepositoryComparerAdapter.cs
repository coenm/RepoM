namespace RepoM.App;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class RepositoryComparerManager
{
    private readonly IAppSettingsService _appSettingsService;
    private ComparerComposition _comparer;

    public RepositoryComparerManager(IAppSettingsService appSettingsService)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _comparer = new ComparerComposition(new Dictionary<string, IComparer>());
        ViewModel = new OrderingsViewModel(appSettingsService);
    }

    public IComparer Comparer => _comparer;

    public OrderingsViewModel ViewModel { get; }
}

internal class ComparerComposition : IComparer
{
    private readonly Dictionary<string, IComparer> _comparers;
    private IComparer _selected;

    public ComparerComposition(Dictionary<string, IComparer> namedComparers)
    {
        _comparers = namedComparers;
        _selected = _comparers.First().Value;
    }

    public bool SetComparer(string key)
    {
        if (_comparers.TryGetValue(key, out IComparer? value))
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