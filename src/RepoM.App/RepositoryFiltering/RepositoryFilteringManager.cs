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
    private readonly ILogger _logger;
    private readonly QueryParserComposition _queryParser;
    private readonly List<string> _repositoryComparerKeys;
    private readonly List<string> _preFilterKeys;
    private readonly Dictionary<string, RepositoryFilterConfiguration> _queryDictionary;

    public RepositoryFilteringManager(
        IAppSettingsService appSettingsService,
        IFilterSettingsService filterSettingsService,
        IEnumerable<INamedQueryParser> queryParsers,
        ILogger logger)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _ = queryParsers ?? throw new ArgumentNullException(nameof(queryParsers));
        _ = filterSettingsService ?? throw new ArgumentNullException(nameof(filterSettingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        INamedQueryParser[] queryParsersArray = queryParsers.ToArray();
        if (queryParsersArray.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(queryParsers));
        }

        INamedQueryParser defaultParser = queryParsersArray.First(x => x.Name != "Lucene");
        INamedQueryParser queryParser = Array.Find(queryParsersArray, x => x.Name == "Lucene") ?? defaultParser;

        _queryDictionary = new Dictionary<string, RepositoryFilterConfiguration>((int)StringComparison.CurrentCultureIgnoreCase);
        foreach (var (key, value) in filterSettingsService.Configuration)
        {
            _queryDictionary.Add(key, new RepositoryFilterConfiguration
            {
                AlwaysVisible = Map(value.AlwaysVisible),
                Description = value.Description,
                Filter = Map(value.Filter),
                Name = key,
            });
        }

        var allConfiguration = new RepositoryFilterConfiguration
        {
            Name = "All",
            AlwaysVisible = null,
            Description = "Show all (no filtering) [Default]",
            Filter = null,
        };

        if (!_queryDictionary.TryAdd(allConfiguration.Name, allConfiguration))
        {
            _logger.LogWarning("Invalid filter config detected: {FilterName}. Overwriting element.", allConfiguration.Name);
            _queryDictionary[allConfiguration.Name] = allConfiguration;
        }

        _preFilterKeys = _queryDictionary.Keys.ToList();

        _queryParser = new QueryParserComposition(queryParsersArray);

        _repositoryComparerKeys = queryParsersArray.Select(x => x.Name).ToList();

        PreFilter = TrueQuery.Instance;

        if (string.IsNullOrWhiteSpace(_appSettingsService.QueryParserKey))
        {
            _logger.LogInformation("Query parser was not set. Pick default one one.");
            SetQueryParser(_repositoryComparerKeys[0]);
        }
        else if (!SetQueryParser(_appSettingsService.QueryParserKey))
        {
            _logger.LogInformation("Could not set query parser '{Key}'. Falling back to default parser.", _appSettingsService.QueryParserKey);
            SetQueryParser(_repositoryComparerKeys[0]);
        }

        if (_queryDictionary.TryGetValue("Default", out RepositoryFilterConfiguration? defaultFilterConfig) && !string.IsNullOrWhiteSpace(_appSettingsService.SelectedFilter))
        {
            SetFilter(defaultFilterConfig);
        }
        else
        {
            SetFilter(allConfiguration);
        }

        return;

        IQuery? Map(QueryConfiguration input)
        {
            if (string.IsNullOrWhiteSpace(input.Query))
            {
                return null;
            }

            if ("query@1".Equals(input.Kind, StringComparison.CurrentCulture))
            {
                return queryParser.Parse(input.Query);
            }

            return defaultParser.Parse(input.Query);
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
            _logger.LogWarning("Could not update/set the comparer key {Key}.", key);
            return false;
        }

        _appSettingsService.QueryParserKey = key;
        SelectedQueryParserKey = key;
        SelectedQueryParserChanged?.Invoke(this, key);
        return true;
    }

    private void SetFilter(RepositoryFilterConfiguration filterConfig)
    {
        PreFilter = filterConfig.Filter ?? TrueQuery.Instance;
        AlwaysVisibleFilter = filterConfig.AlwaysVisible;
        _appSettingsService.SelectedFilter = filterConfig.Name;
        SelectedFilterKey = filterConfig.Name;
        SelectedFilterChanged?.Invoke(this, filterConfig.Name);
    }

    public bool SetFilter(string filterName)
    {
        if (_queryDictionary.TryGetValue(filterName, out RepositoryFilterConfiguration? filterConfig))
        {
            SetFilter(filterConfig);
            return true;
        }

        _logger.LogWarning("Could not find filter with filterName {Key}.", filterName);
        return false;
    }

    private sealed class RepositoryFilterConfiguration
    {
        public string Name { get; init; } = null!;

        public string Description { get; init; } = null!;

        public IQuery? AlwaysVisible { get; init; }

        public IQuery? Filter { get; init; }
    }
}