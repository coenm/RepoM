namespace RepoM.App.RepositoryOrdering;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RepoM.Api.Common;
using RepoM.Api.Ordering.Az;
using RepoM.Core.Plugin.RepositoryOrdering;
using RepoM.Core.Plugin.RepositoryOrdering.Configuration;

internal class RepositoryComparerManager : IRepositoryComparerManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly ComparerComposition _comparer;
    private readonly List<string> _repositoryComparerKeys;

    public RepositoryComparerManager(
        IAppSettingsService appSettingsService,
        ICompareSettingsService compareSettingsService,
        IRepositoryComparerFactory repositoryComparerFactory)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));

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

        SelectedRepositoryComparerKey = _repositoryComparerKeys.First();
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

        SelectedRepositoryComparerKey = key;
        SelectedRepositoryComparerKeyChanged?.Invoke(this, key);
        return true;

    }
}