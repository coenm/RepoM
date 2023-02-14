namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class RepositoryFilteringManager : IRepositoryFilteringManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly INamedQueryParser[] _queryParsers;
    private readonly ILogger _logger;
    private readonly QueryParserComposition _queryParser;
    private readonly List<string> _repositoryComparerKeys;
    private readonly List<string> _preFilterKeys;
    private readonly ConcurrentDictionary<string, IQuery> _queryDictionary;

    public RepositoryFilteringManager(
        IAppSettingsService appSettingsService,
        ICompareSettingsService compareSettingsService,
        IRepositoryComparerFactory repositoryComparerFactory,
        IEnumerable<INamedQueryParser> queryParsers,
        ILogger logger)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _queryParsers = queryParsers.ToArray() ?? throw new ArgumentNullException(nameof(queryParsers));
        _ = compareSettingsService ?? throw new ArgumentNullException(nameof(compareSettingsService));
        _ = repositoryComparerFactory ?? throw new ArgumentNullException(nameof(repositoryComparerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_queryParsers.Any())
        {
            throw new ArgumentOutOfRangeException("Cannot be empty", nameof(queryParsers));
        }

        // tmp
        _queryDictionary = new ConcurrentDictionary<string, IQuery>();
        _queryDictionary.TryAdd("Default", new TrueQuery());
        _queryDictionary.TryAdd("TIS", new AndQuery(new SimpleTerm("tag", "TIS")));
        _queryDictionary.TryAdd("DRC", new AndQuery(new SimpleTerm("tag", "DRC")));
        _queryDictionary.TryAdd("Prive", new AndQuery(new SimpleTerm("tag", "Prive")));
        _preFilterKeys = _queryDictionary.Keys.ToList();

        _queryParser = new QueryParserComposition(_queryParsers);

        _repositoryComparerKeys = _queryParsers.Select(x => x.Name).ToList();

        PreFilter = new TrueQuery();
        PreFilter = new AndQuery(new SimpleTerm("tag", "DRC"));

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

        KeyValuePair<string, IQuery> first = _queryDictionary.First();

        if (string.IsNullOrWhiteSpace(_appSettingsService.SelectedFilter))
        {
            SetFilter(first.Key);
        }
        else if (!SetFilter(_appSettingsService.SelectedFilter))
        {
            SetFilter(first.Key);
        }
    }

    public event EventHandler<string>? SelectedQueryParserChanged;

    public event EventHandler<string>? SelectedFilterChanged;

    public IQueryParser QueryParser => _queryParser;

    public IQuery PreFilter { get; private set; }

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
        if (!_queryDictionary.TryGetValue(key, out IQuery? value))
        {
            return false;
        }

        PreFilter = value;
        _appSettingsService.SelectedFilter = key;
        SelectedFilterKey = key;
        SelectedFilterChanged?.Invoke(this, key);
        return true;
    }
}