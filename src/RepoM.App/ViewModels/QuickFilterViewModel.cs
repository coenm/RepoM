namespace RepoM.App.ViewModels;

using System;
using System.ComponentModel;
using System.Windows.Input;
using RepoM.Api.QuickFilter;

public sealed class QuickFilterViewModel : INotifyPropertyChanged
{
    private readonly QuickFilterModel _model;
    private readonly IQuickFilterService _service;

    public QuickFilterViewModel(QuickFilterModel model, IQuickFilterService service)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        ToggleCommand = new RelayCommand(_ => Toggle());
        RemoveCommand = new RelayCommand(_ => _service.Remove(_model.Id));
        EditLabelCommand = new RelayCommand(_ => { /* Handled in code-behind with input dialog */ });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id => _model.Id;

    public string Label => _model.Label;

    public string DisplayLabel => _model.Label.Length > 10 ? _model.Label[..10] + "..." : _model.Label;

    public string ToolTip => _model.IsBuiltIn
        ? _model.Label == "\u2605" ? "Favorites" : "Active"
        : string.IsNullOrEmpty(_model.ToolTip) ? _model.Label : _model.ToolTip;

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInverse)));
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Order)));
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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayLabel)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolTip)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasToolTip)));
    }

    public void UpdateToolTip(string newToolTip)
    {
        _service.UpdateToolTip(_model.Id, newToolTip);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToolTip)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RawToolTip)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasToolTip)));
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
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute(parameter);
    }
}
