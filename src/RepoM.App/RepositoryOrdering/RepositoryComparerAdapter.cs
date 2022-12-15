namespace RepoM.App.RepositoryOrdering;

using System;
using System.Collections;
using RepoM.Api.Git;
using RepoM.Core.Plugin.RepositoryOrdering;

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