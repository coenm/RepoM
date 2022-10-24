namespace RepoM.App;

using System.Collections;
using RepoM.Api.Git;

internal class CustomRepositoryViewSortComparer : IComparer
{
    public int Compare(object? x, object? y)
    {
        if (x is IRepositoryView xView && y is IRepositoryView yView)
        {
            return Compare(xView, yView);
        }

        return 0;
    }

    private static int Compare(IRepositoryView x, IRepositoryView y)
    {
        if (x.IsPinned == y.IsPinned)
        {
            return string.CompareOrdinal(x.Name, y.Name);
        }

        // pinned should be first ;-)
        return x.IsPinned ? -1 : 1;
    }
}