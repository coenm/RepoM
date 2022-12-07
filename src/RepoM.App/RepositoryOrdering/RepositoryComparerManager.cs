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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (compareSettingsService == null)
        {
            throw new ArgumentNullException(nameof(compareSettingsService));
        }

        if (repositoryComparerFactory == null)
        {
            throw new ArgumentNullException(nameof(repositoryComparerFactory));
        }
        
        Dictionary<string, IRepositoriesComparerConfiguration> multipleConfigurations = new ();
        var comparers = new Dictionary<string, IComparer>();

        try
        {
            multipleConfigurations = compareSettingsService.Configuration;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        foreach ((var key, IRepositoriesComparerConfiguration config) in multipleConfigurations)
        {
            try
            {
                if (!comparers.TryAdd(key, new RepositoryComparerAdapter(repositoryComparerFactory.Create(config))))
                {
                    // swallow
                }
            }
            catch (Exception)
            {
                // swallow
            }
        }

        if (comparers.Count == 0)
        {
            comparers.Add("Default", new RepositoryComparerAdapter(new AzComparer(1, "Name")));
        }
        
        _comparer = new ComparerComposition(comparers);

        _repositoryComparerKeys = comparers.Select(x => x.Key).ToList();

        if (!SetRepositoryComparer(_appSettingsService.SortKey))
        {
            SetRepositoryComparer(_repositoryComparerKeys.First());
        }
    }

    public event EventHandler<string>? SelectedRepositoryComparerKeyChanged;

    public IComparer Comparer => _comparer;

    public string SelectedRepositoryComparerKey { get; private set; }

    public IReadOnlyList<string> RepositoryComparerKeys => _repositoryComparerKeys;

    public bool SetRepositoryComparer(string key)
    {
        if (!_comparer.SetComparer(key))
        {
            return false;
        }

        _appSettingsService.SortKey = key;
        SelectedRepositoryComparerKey = key;
        SelectedRepositoryComparerKeyChanged?.Invoke(this, key);
        return true;
    }
}