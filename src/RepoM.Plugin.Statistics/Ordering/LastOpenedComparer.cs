namespace RepoM.Plugin.Statistics.Ordering;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class LastOpenedComparer : IRepositoryComparer
{
    private readonly IStatisticsService _service;
    private readonly int _weight;
    private readonly int _negativeWeight;
    
    public LastOpenedComparer(IStatisticsService service, int weight)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _weight = weight;
        _negativeWeight = -1 * weight;
    }

    public int Compare(IRepository? x, IRepository? y)
    {
        if (_weight == 0)
        {
            return 0;
        }

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
            return _negativeWeight;
        }

        DateTime lastX = GetLast(x);
        DateTime lastY = GetLast(y);

        if (lastX == lastY)
        {
            return 0;
        }

        if (lastX < lastY)
        {
            return _weight;
        }

        return _negativeWeight;
    }

    private DateTime GetLast(IRepository repository)
    {
        IReadOnlyList<DateTime> items = _service.GetRecordings(repository);

        // TODO lots of crashes here
        return items.Count == 0
            ? DateTime.MinValue
            : items.MaxBy(x => x);
    }
}