namespace RepoM.App;

using System.Windows;
using System.Windows.Input;
using RepoM.App.Services;

/// <summary>
///     Provides bindable properties and commands for the NotifyIcon. In this sample, the
///     view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
///     in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
/// </summary>
public class NotifyIconViewModel
{
    private const string APP_NAME = "RepoM";

#pragma warning disable CA1822
    /// <summary>
    ///     Shows a window, if none is already open.
    /// </summary>
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public ICommand OpenCommand =>
        new DelegateCommand
            {
                CanExecuteFunc = () => (Application.Current.MainWindow as MainWindow)?.IsShown == false,
                CommandAction  = () => (Application.Current.MainWindow as MainWindow)?.ShowAndActivate(),
            };

    public ICommand StartWithWindows =>
        new DelegateCommand
            {
                CanExecuteFunc = () => !AutoStart.IsStartup(APP_NAME),
                CommandAction  = () => AutoStart.SetStartup(APP_NAME, true),
            };

    public ICommand DoNotStartWithWindows =>
        new DelegateCommand
            {
                CanExecuteFunc = () => AutoStart.IsStartup(APP_NAME),
                CommandAction  = () => AutoStart.SetStartup(APP_NAME, false),
            };

    /// <summary>
    ///     Shuts down the application.
    /// </summary>

    public ICommand ExitApplicationCommand =>
        new DelegateCommand
            {
                CommandAction = () => Application.Current.Shutdown(),
            };
#pragma warning restore CA1822
}