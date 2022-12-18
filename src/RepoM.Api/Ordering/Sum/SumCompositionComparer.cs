namespace RepoM.Api.Ordering.Sum;

using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;

public class SumCompositionComparer : IRepositoryComparer
{
    private readonly IRepositoryComparer[] _comparers;

    public SumCompositionComparer(IEnumerable<IRepositoryComparer> comparers)
    {
        _comparers = comparers.ToArray();
    }

    public int Compare(IRepository? x, IRepository? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (y is null)
        {
            return 1;
        }

        if (x is null)
        {
            return -1;
        }

        return _comparers.Sum(c => c.Compare(x, y));
    }
}