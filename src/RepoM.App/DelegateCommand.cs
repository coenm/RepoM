namespace RepoM.App;

using System;
using System.Windows.Input;

/// <summary>
/// Simplistic delegate command for the demo.
/// </summary>
public class DelegateCommand : ICommand
{
    public Action? CommandAction { get; set; }

    public Func<bool>? CanExecuteFunc { get; set; }

    public void Execute(object? parameter)
    {
        CommandAction?.Invoke();
    }

    public bool CanExecute(object? parameter)
    {
        return CanExecuteFunc?.Invoke() ?? true;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}