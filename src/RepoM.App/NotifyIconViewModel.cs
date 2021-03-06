namespace RepoM.App;

using System;
using System.Windows;
using System.Windows.Input;

/// <summary>
/// Provides bindable properties and commands for the NotifyIcon. In this sample, the
/// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
/// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
/// </summary>
public class NotifyIconViewModel
{
    /// <summary>
    /// Shows a window, if none is already open.
    /// </summary>
    public ICommand OpenCommand =>
        new DelegateCommand
            {
                CanExecuteFunc = () => (Application.Current.MainWindow as MainWindow)?.IsShown == false,
                CommandAction = () => (Application.Current.MainWindow as MainWindow)?.ShowAndActivate(),
            };

    public ICommand StartWithWindows =>
        new DelegateCommand
            {
                CanExecuteFunc = () => !AutoStart.IsStartup("RepoM"),
                CommandAction = () => AutoStart.SetStartup("RepoM", true),
            };

    public ICommand DoNotStartWithWindows =>
        new DelegateCommand
            {
                CanExecuteFunc = () => AutoStart.IsStartup("RepoM"),
                CommandAction = () => AutoStart.SetStartup("RepoM", false),
            };

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    public ICommand ExitApplicationCommand =>
        new DelegateCommand
            {
                CommandAction = () => Application.Current.Shutdown(),
            };
}

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