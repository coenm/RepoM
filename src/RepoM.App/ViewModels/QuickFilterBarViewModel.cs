namespace RepoM.App.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using RepoM.Api.QuickFilter;
using RepoM.App.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Plugin.RepositoryFiltering.Clause.Terms;

public sealed class QuickFilterBarViewModel : INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs _combineModeChangedArgs = new(nameof(CombineMode));
    private static readonly PropertyChangedEventArgs _combineModeLabelChangedArgs = new(nameof(CombineModeLabel));
    private static readonly PropertyChangedEventArgs _combineModeToolTipChangedArgs = new(nameof(CombineModeToolTip));
    private static readonly PropertyChangedEventArgs _hasItemsChangedArgs = new(nameof(HasItems));

    private readonly IQuickFilterService _service;
    private readonly IRepositoryFilteringManager _repositoryFilteringManager;
    private readonly INamedQueryParser[] _queryParsers;
    private readonly ILogger _logger;

    public QuickFilterBarViewModel(
        IQuickFilterService service,
        IRepositoryFilteringManager repositoryFilteringManager,
        System.Collections.Generic.IEnumerable<INamedQueryParser> queryParsers,
        ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _repositoryFilteringManager = repositoryFilteringManager ?? throw new ArgumentNullException(nameof(repositoryFilteringManager));
        _queryParsers = queryParsers?.ToArray() ?? throw new ArgumentNullException(nameof(queryParsers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_queryParsers.Length == 0)
        {
            throw new ArgumentException("At least one query parser must be available.", nameof(queryParsers));
        }

        Items = new ObservableCollection<QuickFilterViewModel>(
            _service.GetAll().Select(model => new QuickFilterViewModel(model, _service)));

        AddTagCommand = new RelayCommand(parameter =>
        {
            if (parameter is string tag)
            {
                AddFromTag(tag);
            }
        });

        SaveSearchTextCommand = new RelayCommand(parameter =>
        {
            if (parameter is string searchText)
            {
                SaveFromSearchText(searchText);
            }
        });

        ToggleCombineModeCommand = new RelayCommand(_ => ToggleCombineMode());

        _service.Changed += OnServiceChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? FilterStateChanged;

    public ObservableCollection<QuickFilterViewModel> Items { get; }

    public bool HasItems => Items.Count > 0;

    public ICommand AddTagCommand { get; }

    public ICommand SaveSearchTextCommand { get; }

    public ICommand ToggleCombineModeCommand { get; }

    public QuickFilterCombineMode CombineMode
    {
        get => _service.CombineMode;
        set
        {
            if (_service.CombineMode == value)
            {
                return;
            }

            _service.CombineMode = value;
            PropertyChanged?.Invoke(this, _combineModeChangedArgs);
            PropertyChanged?.Invoke(this, _combineModeLabelChangedArgs);
            PropertyChanged?.Invoke(this, _combineModeToolTipChangedArgs);
            FilterStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string CombineModeLabel => _service.CombineMode == QuickFilterCombineMode.And ? "AND" : "OR";

    public string CombineModeToolTip => _service.CombineMode == QuickFilterCombineMode.And
        ? "Filters combined with AND (all must match). Click to switch to OR."
        : "Filters combined with OR (any must match). Click to switch to AND.";

    public IQuery? GetCombinedActiveQuery()
    {
        var activeQueries = _service.GetAll()
            .Where(filter => filter.IsActive)
            .Select(filter => filter.IsInverse ? (IQuery)new NotQuery(filter.Query) : filter.Query)
            .ToArray();

        return activeQueries.Length switch
        {
            0 => null,
            1 => activeQueries[0],
            _ => _service.CombineMode == QuickFilterCombineMode.Or
                ? new OrQuery(activeQueries)
                : new AndQuery(activeQueries),
        };
    }

    public void ToggleCombineMode()
    {
        CombineMode = _service.CombineMode == QuickFilterCombineMode.And
            ? QuickFilterCombineMode.Or
            : QuickFilterCombineMode.And;
    }

    public void AddFromTag(string tag)
    {
        var query = new SimpleTerm("tag", tag);
        var existing = _service.FindByQuery(query);
        if (existing != null)
        {
            _service.SetActive(existing.Id, true);
        }
        else
        {
            _service.Add(tag, query);
        }
    }

    public void AddFromSearchQuery(string label, IQuery query)
    {
        var existing = _service.FindByQuery(query);
        if (existing != null)
        {
            _service.SetActive(existing.Id, true);
        }
        else
        {
            _service.Add(label, query);
        }
    }

    public void Remove(Guid id)
    {
        _service.Remove(id);
    }

    private void SaveFromSearchText(string searchText)
    {
        searchText = searchText?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return;
        }

        try
        {
            var selectedKey = _repositoryFilteringManager.SelectedQueryParserKey;
            IQueryParser? activeParser = _queryParsers.FirstOrDefault(parser => parser.Name == selectedKey);
            activeParser ??= _queryParsers[0];

            var query = activeParser.Parse(searchText);
            AddFromSearchQuery(searchText, query);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save search as quick filter: invalid query.");
        }
    }

    private void OnServiceChanged(object? sender, EventArgs e)
    {
        Items.Clear();
        foreach (var model in _service.GetAll())
        {
            Items.Add(new QuickFilterViewModel(model, _service));
        }

        PropertyChanged?.Invoke(this, _hasItemsChangedArgs);
        FilterStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public event EventHandler? CanExecuteChanged
        {
            add => _ = value;
            remove => _ = value;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute(parameter);
    }
}