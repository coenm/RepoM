namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.Ordering.Az;
using RepoM.App.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

internal class RepositoryFilteringManager : IRepositoryFilteringManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger _logger;
    private readonly QueryParserComposition _queryParser;
    private readonly List<string> _repositoryComparerKeys;

    public RepositoryFilteringManager(
        IAppSettingsService appSettingsService,
        ICompareSettingsService compareSettingsService,
        IRepositoryComparerFactory repositoryComparerFactory,
        ILogger logger)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _ = compareSettingsService ?? throw new ArgumentNullException(nameof(compareSettingsService));
        _ = repositoryComparerFactory ?? throw new ArgumentNullException(nameof(repositoryComparerFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Dictionary<string, IRepositoriesComparerConfiguration> multipleConfigurations = new ();
        var comparers = new Dictionary<string, IQueryParser>();

        try
        {
            multipleConfigurations = compareSettingsService.Configuration;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not get comparer configuration. Falling back to default. {message}", e.Message);
        }
        
        foreach ((var key, IRepositoriesComparerConfiguration config) in multipleConfigurations)
        {
            // try
            // {
            //     if (!comparers.TryAdd(key, new RepositoryComparerAdapter(repositoryComparerFactory.Create(config))))
            //     {
            //         _logger.LogWarning("Could not add comparer for key '{key}'.", key);
            //     }
            // }
            // catch (Exception e)
            // {
            //     _logger.LogError(e, "Could not create a repository comparer for key '{key}'. {message}", key, e.Message);
            // }
        }

        if (comparers.Count == 0)
        {
            // comparers.Add("Default", new RepositoryComparerAdapter(new AzComparer(1, "Name")));
            _logger.LogInformation("No custom comparers added, add default comparer");
        }
        
        _queryParser = new QueryParserComposition(comparers);

        _repositoryComparerKeys = comparers.Select(x => x.Key).ToList();

        if (string.IsNullOrWhiteSpace(_appSettingsService.QueryParserKey))
        {
            _logger.LogInformation("Custom sorter key was not set. Pick first one.");
            SetQueryParser(_repositoryComparerKeys.First());
        }
        else if (!SetQueryParser(_appSettingsService.QueryParserKey))
        {
            _logger.LogInformation("Could not set comparer '{key}'. Falling back to first comparer.", _appSettingsService.QueryParserKey);
            SetQueryParser(_repositoryComparerKeys.First());
        }
    }

    public event EventHandler<string>? SelectedQueryParserChanged;

    public IQueryParser QueryParser => _queryParser;

    public string SelectedQueryParserKey { get; private set; } = "Default";

    public IReadOnlyList<string> QueryParserKeys => _repositoryComparerKeys;

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