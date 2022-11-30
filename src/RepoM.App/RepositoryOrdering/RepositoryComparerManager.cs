namespace RepoM.App.RepositoryOrdering;

using System;
using System.Collections;
using System.Collections.Generic;
using RepoM.Api.Common;
using RepoM.Api.Ordering.Az;
using RepoM.Api.Ordering.Composition;
using RepoM.Api.Ordering.Label;
using RepoM.Api.Ordering.Score;
using RepoM.Core.Plugin.RepositoryOrdering;

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
        
        _repositoryComparerKeys = new List<string>
            {
                "Default",
                "Prive",
                "Config",
            };

        _comparer = new ComparerComposition(
            new Dictionary<string, IComparer>
            {
                { "Default", new RepositoryComparerAdapter(new AzComparer(1, "Name")) },
                { "Prive", new RepositoryComparerAdapter(
                    new CompositionComparer(
                        new IRepositoryComparer[]
                            {
                                new ScoreComparer(new TagScoreCalculator("Prive", 1)),
                                new ScoreComparer(new TagScoreCalculator("TIS", 1)),
                                new AzComparer(1, "Name"),
                            }))},
                { "Config", new RepositoryComparerAdapter(repositoryComparerFactory.Create(compareSettingsService.Configuration)) },
            });

        SelectedRepositoryComparerKey = "Default";
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