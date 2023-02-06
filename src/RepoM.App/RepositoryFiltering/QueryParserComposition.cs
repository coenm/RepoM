namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using YamlDotNet.Core.Tokens;

internal class QueryParserComposition : IQueryParser
{
    private readonly INamedQueryParser[] _namedQueryParsers;
    private IQueryParser _selected;
    
    public QueryParserComposition(INamedQueryParser[] namedNamedQueryParsers)
    {
        _namedQueryParsers = namedNamedQueryParsers;
        _selected = _namedQueryParsers.First();
    }

    public bool SetComparer(string key)
    {
        INamedQueryParser? foundQueryParser = _namedQueryParsers.FirstOrDefault(x => x.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
        
        if (foundQueryParser != null)
        {
            _selected = foundQueryParser;
        }

        return foundQueryParser != null;
    }

    public IQuery Parse(string text)
    {
        IQueryParser queryParser = _selected;
        return queryParser.Parse(text);
    }
}