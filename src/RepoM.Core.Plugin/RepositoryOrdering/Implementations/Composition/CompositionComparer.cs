namespace RepoM.Core.Plugin.RepositoryOrdering.Implementations.Composition;

using System.Collections.Generic;
using System.Linq;

public class CompositionComparer : IRepositoryComparer
{
    private readonly IRepositoryComparer[] _comparers;

    public CompositionComparer(IEnumerable<IRepositoryComparer> comparers)
    {
        _comparers = comparers.ToArray();
    }

    public int Compare(IRepository x, IRepository y)
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