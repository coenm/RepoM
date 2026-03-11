namespace RepoM.App.RepositoryFiltering;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Configuration;

internal class RepositoryFilteringManager : IRepositoryFilteringManager
{
    private readonly IAppSettingsService _appSettingsService;
    private readonly IRepositoryMatcher _repositoryMatcher;
    private readonly ILogger _logger;
    private readonly QueryParserComposition _queryParser;
    private readonly List<string> _repositoryComparerKeys;
    private readonly List<string> _preFilterKeys;
    private readonly List<RepositoryFilterConfiguration> _queryDictionary;

    public RepositoryFilteringManager(
        IAppSettingsService appSettingsService,
        IFilterSettingsService filterSettingsService,
        IEnumerable<INamedQueryParser> queryParsers,
        IRepositoryMatcher repositoryMatcher,
        ILogger logger)
    {
        _appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        _repositoryMatcher = repositoryMatcher ?? throw new ArgumentNullException(nameof(repositoryMatcher));
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

        _queryDictionary = filterSettingsService.Configuration
                                                .Select(x => new RepositoryFilterConfiguration
                                                    {
                                                        AlwaysVisible = Map(x.Value.AlwaysVisible),
                                                        Description = x.Value.Description,
                                                        Filter = Map(x.Value.Filter),
                                                        Name = x.Key,
                                                    })
                                                .ToList();

        if (!_queryDictionary.Exists(x => x.Name.Equals("Default", StringComparison.CurrentCultureIgnoreCase)))
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

        _queryParser = new QueryParserComposition(queryParsersArray);

        _repositoryComparerKeys = queryParsersArray.Select(x => x.Name).ToList();

        PreFilter = TrueQuery.Instance;

        if (string.IsNullOrWhiteSpace(_appSettingsService.QueryParserKey))
        {
            _logger.LogInformation("Query parser was not set. Pick first one.");
            SetQueryParser(_repositoryComparerKeys[0]);
        }
        else if (!SetQueryParser(_appSettingsService.QueryParserKey))
        {
            _logger.LogInformation("Could not set query parser '{Key}'. Falling back to first query parser.", _appSettingsService.QueryParserKey);
            SetQueryParser(_repositoryComparerKeys[0]);
        }

        RepositoryFilterConfiguration first = _queryDictionary[0];

        if (string.IsNullOrWhiteSpace(_appSettingsService.SelectedFilter))
        {
            SetFilter(first.Name);
        }
        else if (!SetFilter(_appSettingsService.SelectedFilter))
        {
            SetFilter(first.Name);
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

    private IQueryParser QueryParser => _queryParser;

    private IQuery PreFilter { get; set; }

    private IQuery? AlwaysVisibleFilter { get; set; }

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

    public bool SetFilter(string key)
    {
        RepositoryFilterConfiguration? value = _queryDictionary.Find(x => x.Name == key);
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

    public IObservable<Func<RepositoryViewModel, bool>> CreateFilterObservable(IObservable<string> textInput)
    {
        ArgumentNullException.ThrowIfNull(textInput);

        var settingsChanged = Observable.Merge(
                Observable.FromEventPattern<EventHandler<string>, string>(
                    h => SelectedFilterChanged += h,
                    h => SelectedFilterChanged -= h),
                Observable.FromEventPattern<EventHandler<string>, string>(
                    h => SelectedQueryParserChanged += h,
                    h => SelectedQueryParserChanged -= h))
            .Select(_ => System.Reactive.Unit.Default)
            .StartWith(System.Reactive.Unit.Default);

        return textInput
            .CombineLatest(settingsChanged, (query, _) => query)
            .Select(CreateFilterPredicate);
    }

    private Func<RepositoryViewModel, bool> CreateFilterPredicate(string query)
    {
        // Capture current filter state so the predicate is self-contained and thread-safe.
        IQuery preFilter = PreFilter;
        IQuery? alwaysVisibleFilter = AlwaysVisibleFilter;
        IQueryParser queryParser = QueryParser;

        IQuery? parsedQuery = null;
        if (!string.IsNullOrWhiteSpace(query))
        {
            try
            {
                parsedQuery = queryParser.Parse(query);
            }
            catch (Exception)
            {
                // Invalid query syntax (e.g. incomplete Lucene expression like "RepoM OR").
                // Return a predicate that hides everything so the user sees the input is invalid.
                return _ => false;
            }
        }

        return vm => MatchesFilter(vm, alwaysVisibleFilter, preFilter, parsedQuery);
    }

    private bool MatchesFilter(RepositoryViewModel vm, IQuery? alwaysVisibleFilter, IQuery preFilter, IQuery? userQuery)
    {
        if (SafeMatches(vm, alwaysVisibleFilter) == true)
        {
            return true;
        }

        if (SafeMatches(vm, preFilter) == false)
        {
            return false;
        }

        return userQuery == null || SafeMatches(vm, userQuery) != false;
    }

    private bool? SafeMatches(RepositoryViewModel vm, IQuery? query)
    {
        if (query == null)
        {
            return null;
        }

        try
        {
            return _repositoryMatcher.Matches(vm.Repository, query);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private sealed class RepositoryFilterConfiguration
    {
        public string Name { get; init; } = null!;

        public string Description { get; init; } = null!;

        public IQuery? AlwaysVisible { get; init; }

        public IQuery? Filter { get; init; }
    }
}