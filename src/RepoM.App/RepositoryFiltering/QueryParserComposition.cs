namespace RepoM.App.RepositoryFiltering;

using System;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

internal class QueryParserComposition : IQueryParser
{
    private readonly INamedQueryParser[] _namedQueryParsers;
    private IQueryParser _selected;
    
    public QueryParserComposition(INamedQueryParser[] namedNamedQueryParsers)
    {
        _namedQueryParsers = namedNamedQueryParsers;
        _selected = _namedQueryParsers[0];
    }

    public bool SetComparer(string key)
    {
        INamedQueryParser? foundQueryParser = Array.Find(_namedQueryParsers, item => item.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
        
        if (foundQueryParser != null)
        {
            _selected = foundQueryParser;
        }

        return foundQueryParser != null;
    }

    public IQuery Parse(string text)
    {
        if (text.Equals(_cacheText))
        {
            return _cacheQuery;
        }

        IQueryParser queryParser = _selected;
        _cacheText = text;
        _cacheQuery = queryParser.Parse(text);
        return _cacheQuery;
    }

    private string _cacheText = string.Empty;

    private IQuery _cacheQuery = new FreeText(string.Empty);
}