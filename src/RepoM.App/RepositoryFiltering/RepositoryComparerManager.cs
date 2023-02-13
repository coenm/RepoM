namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class RepositoryFilteringManager : IRepositoryFilteringManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly INamedQueryParser[] _queryParsers;
    private readonly ILogger _logger;
    private readonly QueryParserComposition _queryParser;
    private readonly List<string> _repositoryComparerKeys;

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

        _queryParser = new QueryParserComposition(_queryParsers);

        _repositoryComparerKeys = _queryParsers.Select(x => x.Name).ToList();

        PreFilter = new TrueQuery();

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
    }

    public event EventHandler<string>? SelectedQueryParserChanged;

    public event EventHandler<string>? PreFilterChanged;

    public IQueryParser QueryParser => _queryParser;

    public string SelectedQueryParserKey { get; private set; } = string.Empty;

    public IReadOnlyList<string> QueryParserKeys => _repositoryComparerKeys;
    
    public IQuery PreFilter { get; private set; }

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
}