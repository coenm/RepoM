namespace RepoM.App;

using System.Collections;
using RepoM.Api.Git;

internal class CustomRepositoryViewSortBehavior : IComparer
{
    public int Compare(object? x, object? y)
    {
        if (x is RepositoryView xView && y is RepositoryView yView)
        {
            return string.CompareOrdinal(xView.Name, yView.Name);
        }

        return 0;
    }
}