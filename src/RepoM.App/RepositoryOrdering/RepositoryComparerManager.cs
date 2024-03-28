namespace RepoM.App.RepositoryOrdering;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.Ordering.Az;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

internal class RepositoryComparerManager : IRepositoryComparerManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ILogger _logger;
    private readonly ComparerComposition _comparer;
    private readonly List<string> _repositoryComparerKeys;

    public RepositoryComparerManager(
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
        var comparers = new Dictionary<string, IComparer>();

        try
        {
            multipleConfigurations = compareSettingsService.Configuration;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not get comparer configuration. Falling back to default. {Message}", e.Message);
        }
        
        foreach ((var key, IRepositoriesComparerConfiguration config) in multipleConfigurations)
        {
            try
            {
                if (!comparers.TryAdd(key, new RepositoryComparerAdapter(repositoryComparerFactory.Create(config))))
                {
                    _logger.LogWarning("Could not add comparer for key '{Key}'.", key);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not create a repository comparer for key '{Key}'. {Message}", key, e.Message);
            }
        }

        if (comparers.Count == 0)
        {
            comparers.Add("Default", new RepositoryComparerAdapter(new AzComparer(1, "Name")));
            _logger.LogInformation("No custom comparers added, add default comparer");
        }
        
        _comparer = new ComparerComposition(comparers);

        _repositoryComparerKeys = comparers.Select(x => x.Key).ToList();

        if (string.IsNullOrWhiteSpace(_appSettingsService.SortKey))
        {
            _logger.LogInformation("Custom sorter key was not set. Pick first one.");
            SetRepositoryComparer(_repositoryComparerKeys[0]);
        }
        else if (!SetRepositoryComparer(_appSettingsService.SortKey))
        {
            _logger.LogInformation("Could not set comparer '{Key}'. Falling back to first comparer.", _appSettingsService.SortKey);
            SetRepositoryComparer(_repositoryComparerKeys[0]);
        }
    }

    public event EventHandler<string>? SelectedRepositoryComparerKeyChanged;

    public IComparer Comparer => _comparer;

    public string SelectedRepositoryComparerKey { get; private set; } = "Default";

    public IReadOnlyList<string> RepositoryComparerKeys => _repositoryComparerKeys;

    public bool SetRepositoryComparer(string key)
    {
        if (!_comparer.SetComparer(key))
        {
            _logger.LogWarning("Could not update/set the comparer key {Key}.", key);
            return false;
        }

        _appSettingsService.SortKey = key;
        SelectedRepositoryComparerKey = key;
        SelectedRepositoryComparerKeyChanged?.Invoke(this, key);
        return true;
    }
}