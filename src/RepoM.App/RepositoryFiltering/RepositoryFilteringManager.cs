namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Configuration;

internal class RepositoryFilteringManager : IRepositoryFilteringManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly INamedQueryParser[] _queryParsers;
    private readonly ILogger _logger;
    private readonly QueryParserComposition _queryParser;
    private readonly List<string> _repositoryComparerKeys;
    private readonly List<string> _preFilterKeys;
    private readonly List<RepositoryFilterConfiguration> _queryDictionary;

    public RepositoryFilteringManager(
        IAppSettingsService appSettingsService,
        IFilterSettingsService filterSettingsService,
        IEnumerable<INamedQueryParser> queryParsers,
        ILogger logger)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _queryParsers = queryParsers.ToArray() ?? throw new ArgumentNullException(nameof(queryParsers));
        _ = filterSettingsService ?? throw new ArgumentNullException(nameof(filterSettingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_queryParsers.Any())
        {
            throw new ArgumentOutOfRangeException("Cannot be empty", nameof(queryParsers));
        }

        INamedQueryParser defaultParser = _queryParsers.First(x => x.Name != "Lucene");
        INamedQueryParser? query1Parser = _queryParsers.FirstOrDefault(x => x.Name == "Lucene");
        if (query1Parser == null)
        {
            query1Parser = defaultParser;
        }

        IQuery? Map(QueryConfiguration input)
        {
            if (string.IsNullOrWhiteSpace(input.Query))
            {
                return null;
            }

            if ("query@1".Equals(input.Kind, StringComparison.CurrentCulture))
            {
                return query1Parser.Parse(input.Query);
            }

            return defaultParser.Parse(input.Query);
        }

        _queryDictionary = filterSettingsService.Configuration
                                                .Select(x => new RepositoryFilterConfiguration
                                                    {
                                                        AlwaysVisible = Map(x.Value.AlwaysVisible),
                                                        Description = x.Value.Description,
                                                        Filter = Map(x.Value.Filter),
                                                        Name = x.Key,
                                                    })
                                                .ToList();

        if (!_queryDictionary.Any(x => x.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase)))
        {
            _queryDictionary.Add(new RepositoryFilterConfiguration
                {
                    AlwaysVisible = null,
                    Description = "Default (no filtering)",
                    Filter = null,
                    Name = "Default",
                });
        }
        
        _preFilterKeys = _queryDictionary.Select(x => x.Name).ToList();

        _queryParser = new QueryParserComposition(_queryParsers);

        _repositoryComparerKeys = _queryParsers.Select(x => x.Name).ToList();

        PreFilter = TrueQuery.Instance;

        if (string.IsNullOrWhiteSpace(_appSettingsService.QueryParserKey))
        {
            _logger.LogInformation("Query parser was not set. Pick first one.");
            SetQueryParser(_repositoryComparerKeys.First());
        }
        else if (!SetQueryParser(_appSettingsService.QueryParserKey))
        {
            _logger.LogInformation("Could not set query parser '{key}'. Falling back to first query parser.", _appSettingsService.QueryParserKey);
            SetQueryParser(_repositoryComparerKeys.First());
        }

        RepositoryFilterConfiguration first = _queryDictionary.First();

        if (string.IsNullOrWhiteSpace(_appSettingsService.SelectedFilter))
        {
            SetFilter(first.Name);
        }
        else if (!SetFilter(_appSettingsService.SelectedFilter))
        {
            SetFilter(first.Name);
        }
    }

    public event EventHandler<string>? SelectedQueryParserChanged;

    public event EventHandler<string>? SelectedFilterChanged;

    public IQueryParser QueryParser => _queryParser;

    public IQuery PreFilter { get; private set; }

    public IQuery? AlwaysVisibleFilter { get; private set; }

    public string SelectedQueryParserKey { get; private set; } = string.Empty;

    public string SelectedFilterKey { get; private set; } = string.Empty;

    public IReadOnlyList<string> QueryParserKeys => _repositoryComparerKeys;

    public IReadOnlyList<string> FilterKeys => _preFilterKeys;

    public bool SetQueryParser(string key)
    {  
        if (!_queryParser.SetComparer(key))
        {
            _logger.LogWarning("Could not update/set the comparer key {key}.", key);
            return false;
        }

        _appSettingsService.QueryParserKey = key;
        SelectedQueryParserKey = key;
        SelectedQueryParserChanged?.Invoke(this, key);
        return true;
    }

    public bool SetFilter(string key)
    {
        RepositoryFilterConfiguration? value = _queryDictionary.FirstOrDefault(x => x.Name == key);
        if (value == null)
        {
            return false;
        }
        
        PreFilter = value.Filter ?? TrueQuery.Instance;
        AlwaysVisibleFilter = value.AlwaysVisible;
        _appSettingsService.SelectedFilter = key;
        SelectedFilterKey = key;
        SelectedFilterChanged?.Invoke(this, key);
        return true;
    }

    private class RepositoryFilterConfiguration
    {
        public string Name { get; init; }

        public string Description { get; init; }

        public IQuery? AlwaysVisible { get; init; }

        public IQuery? Filter { get; init; }
    }
}