namespace RepoM.App.RepositoryOrdering;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

internal class ComparerComposition : IComparer
{
    private readonly Dictionary<string, IComparer> _namedComparers;
    private IComparer _selected;

    public ComparerComposition(Dictionary<string, IComparer> namedNamedComparers)
    {
        _namedComparers = namedNamedComparers;
        _selected = _namedComparers.First().Value;
    }

    public bool SetComparer(string key)
    {
        if (_namedComparers.TryGetValue(key, out IComparer? value))
        {
            _selected = value;
            return true;
        }

        return false;
    }

    public int Compare(object? x, object? y)
    {
        IComparer comparer = _selected;
        return comparer.Compare(x, y);
    }
}