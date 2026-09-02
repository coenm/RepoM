namespace RepoM.App.ViewModels;

using System;
using System.ComponentModel;
using System.Windows.Input;
using RepoM.Api.QuickFilter;

public sealed class QuickFilterViewModel : INotifyPropertyChanged
{
    private const string FavoriteLabel = "\u2605";

    private static readonly PropertyChangedEventArgs _isActiveChangedArgs = new(nameof(IsActive));
    private static readonly PropertyChangedEventArgs _isInverseChangedArgs = new(nameof(IsInverse));
    private static readonly PropertyChangedEventArgs _orderChangedArgs = new(nameof(Order));
    private static readonly PropertyChangedEventArgs _labelChangedArgs = new(nameof(Label));
    private static readonly PropertyChangedEventArgs _displayLabelChangedArgs = new(nameof(DisplayLabel));
    private static readonly PropertyChangedEventArgs _toolTipChangedArgs = new(nameof(ToolTip));
    private static readonly PropertyChangedEventArgs _rawToolTipChangedArgs = new(nameof(RawToolTip));
    private static readonly PropertyChangedEventArgs _hasToolTipChangedArgs = new(nameof(HasToolTip));

    private readonly QuickFilterModel _model;
    private readonly IQuickFilterService _service;

    public QuickFilterViewModel(QuickFilterModel model, IQuickFilterService service)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        ToggleCommand = new RelayCommand(_ => Toggle());
        RemoveCommand = new RelayCommand(_ => _service.Remove(_model.Id));
        EditLabelCommand = new RelayCommand(parameter => _ = parameter);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id => _model.Id;

    public string Label => _model.Label;

    public string DisplayLabel => _model.Label.Length > 10 ? _model.Label[..10] + "..." : _model.Label;

    public string ToolTip
    {
        get
        {
            if (_model.IsBuiltIn)
            {
                return _model.Label == FavoriteLabel ? "Favorites" : "Active";
            }

            return string.IsNullOrEmpty(_model.ToolTip) ? _model.Label : _model.ToolTip;
        }
    }

    public string RawToolTip => _model.IsBuiltIn ? string.Empty : _model.ToolTip;

    public bool HasToolTip => !string.IsNullOrEmpty(ToolTip);

    public bool IsBuiltIn => _model.IsBuiltIn;

    public bool IsActive
    {
        get => _model.IsActive;
        set
        {
            if (_model.IsActive == value)
            {
                return;
            }

            _service.SetActive(_model.Id, value);
            PropertyChanged?.Invoke(this, _isActiveChangedArgs);
        }
    }

    public bool IsInverse
    {
        get => _model.IsInverse;
        set
        {
            if (_model.IsInverse == value)
            {
                return;
            }

            _service.SetInverse(_model.Id, value);
            PropertyChanged?.Invoke(this, _isInverseChangedArgs);
        }
    }

    public int Order
    {
        get => _model.Order;
        set
        {
            if (_model.Order == value)
            {
                return;
            }

            _service.UpdateOrder(_model.Id, value);
            PropertyChanged?.Invoke(this, _orderChangedArgs);
        }
    }

    public ICommand ToggleCommand { get; }

    public ICommand RemoveCommand { get; }

    public ICommand EditLabelCommand { get; }

    public void Toggle()
    {
        if (!IsActive)
        {
            // off → on
            IsInverse = false;
            IsActive = true;
        }
        else if (!IsInverse)
        {
            // on → inverse
            IsInverse = true;
        }
        else
        {
            // inverse → off
            IsInverse = false;
            IsActive = false;
        }
    }

    public void UpdateLabel(string newLabel)
    {
        _service.UpdateLabel(_model.Id, newLabel);
        PropertyChanged?.Invoke(this, _labelChangedArgs);
        PropertyChanged?.Invoke(this, _displayLabelChangedArgs);
        PropertyChanged?.Invoke(this, _toolTipChangedArgs);
        PropertyChanged?.Invoke(this, _hasToolTipChangedArgs);
    }

    public void UpdateToolTip(string newToolTip)
    {
        _service.UpdateToolTip(_model.Id, newToolTip);
        PropertyChanged?.Invoke(this, _toolTipChangedArgs);
        PropertyChanged?.Invoke(this, _rawToolTipChangedArgs);
        PropertyChanged?.Invoke(this, _hasToolTipChangedArgs);
    }

    private sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute)
        {
            _execute = execute;
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
