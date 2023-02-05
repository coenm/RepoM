namespace RepoM.App.RepositoryFiltering;

using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;

internal class QueryParserComposition : IQueryParser
{
    private readonly Dictionary<string, IQueryParser> _namedQueryParsers;
    private IQueryParser _selected;

    public QueryParserComposition(Dictionary<string, IQueryParser> namedNamedQueryParsers)
    {
        _namedQueryParsers = namedNamedQueryParsers;
        _selected = _namedQueryParsers.First().Value;
    }

    public bool SetComparer(string key)
    {
        if (_namedQueryParsers.TryGetValue(key, out IQueryParser? value))
        {
            _selected = value;
            return true;
        }

        return false;
    }

    public IQuery Parse(string text)
    {
        IQueryParser queryParser = _selected;
        return queryParser.Parse(text);
    }
}