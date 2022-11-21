namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations;

using System;

public class AzComparer : IRepositoryComparer
{
    public int Compare(IPluginRepository x, IPluginRepository y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (ReferenceEquals(null, y))
        {
            return 1;
        }

        if (ReferenceEquals(null, x))
        {
            return -1;
        }

        var nameComparison = string.Compare(x.Name, y.Name, StringComparison.Ordinal);

        if (nameComparison < 0)
        {
            return -1;
        }

        if (nameComparison == 0)
        {
            return 0;
        }

        return 1;

        if (nameComparison != 0)
        {
            return nameComparison;
        }

        // var currentBranchComparison = string.Compare(x.CurrentBranch, y.CurrentBranch, StringComparison.Ordinal);
        // if (currentBranchComparison != 0)
        // {
        //     return currentBranchComparison;
        // }
        //
        // var pathComparison = string.Compare(x.Path, y.Path, StringComparison.Ordinal);
        // if (pathComparison != 0)
        // {
        //     return pathComparison;
        // }
        //
        // var isPinnedComparison = x.IsPinned.CompareTo(y.IsPinned);
        // if (isPinnedComparison != 0)
        // {
        //     return isPinnedComparison;
        // }
        //
        // return x.HasUnpushedChanges.CompareTo(y.HasUnpushedChanges);
    }
}