namespace RepoM.App;

using System.Collections;
using RepoM.Api.Git;

internal class CustomRepositoryViewSortBehavior : IComparer
{
    public int Compare(object? x, object? y)
    {
        if (x is RepositoryView xView && y is RepositoryView yView)
        {
            return Compare(xView, yView);
        }

        return 0;
    }

    private static int Compare(RepositoryView x, RepositoryView y)
    {
        if (x.IsPinned == y.IsPinned)
        {
            return string.CompareOrdinal(x.Name, y.Name);
        }

        // pinned should be first ;-)
        return x.IsPinned ? -1 : 1;
    }
}