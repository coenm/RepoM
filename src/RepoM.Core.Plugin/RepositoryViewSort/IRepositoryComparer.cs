namespace RepoM.Core.Plugin.RepositoryViewSort;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

public interface IRepositoryComparer : IComparer<IPluginRepository>
{
}

public class SumCompositionComparer : IRepositoryComparer
{
    private readonly IRepositoryComparer[] _comparers;

    public SumCompositionComparer(IEnumerable<IRepositoryComparer> comparers)
    {
        _comparers = comparers.ToArray();
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

        return _comparers.Sum(c => c.Compare(x, y));
    }
}

public class CompositionComparer : IRepositoryComparer
{
    private readonly IRepositoryComparer[] _comparers;

    public CompositionComparer(IEnumerable<IRepositoryComparer> comparers)
    {
        _comparers = comparers.ToArray();
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

        return _comparers.Select(c => c.Compare(x, y)).FirstOrDefault(result => result != 0);
    }
}




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

        return (x.IsPinned.CompareTo(y.IsPinned) * _weight);
    }
}

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

public interface IRepositoryComparerFactory
{
}

public interface IPluginRepository
{
    string Name { get; }

    string CurrentBranch { get; }

    string Path { get; }

    bool IsPinned { get; }

    bool HasUnpushedChanges { get; }
}