namespace RepoM.Api.Ordering.Az;

using System;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;

public class AzComparer : IRepositoryComparer
{
    private readonly int _weight;
    private readonly string? _property;

    public AzComparer(int weight, string? property)
    {
        _weight = weight;
        _property = property;
    }

    public int Compare(IRepository? x, IRepository? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (y is null)
        {
            return _weight;
        }

        if (x is null)
        {
            return -1 * _weight;
        }

        var comparisonValue = 0;

        if ("Name".Equals(_property, StringComparison.InvariantCultureIgnoreCase))
        {
            comparisonValue = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
        else if ("Location".Equals(_property, StringComparison.InvariantCultureIgnoreCase))
        {
            comparisonValue = string.Compare(x.Location, y.Location, StringComparison.Ordinal);
        }

        if (comparisonValue < 0)
        {
            return -1 * _weight;
        }

        if (comparisonValue == 0)
        {
            return 0;
        }

        return _weight;
    }
}