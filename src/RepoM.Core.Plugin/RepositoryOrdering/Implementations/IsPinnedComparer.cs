namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations;

using System;

public class IsPinnedComparer : IRepositoryComparer
{
    private readonly int _weight;

    public IsPinnedComparer(int weight)
    {
        if (weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Should be positive");
        }

        _weight = weight;
    }

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

        return x.IsPinned.CompareTo(y.IsPinned) * _weight;
    }
}