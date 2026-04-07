namespace RepoM.App.ViewModels;

using System;
using System.Windows.Input;

internal sealed class MainWindowQuickFilterCommands
{
    public MainWindowQuickFilterCommands(ICommand saveQuickFilterCommand, ICommand addQuickFilterTagCommand)
    {
        SaveQuickFilterCommand = saveQuickFilterCommand ?? throw new ArgumentNullException(nameof(saveQuickFilterCommand));
        AddQuickFilterTagCommand = addQuickFilterTagCommand ?? throw new ArgumentNullException(nameof(addQuickFilterTagCommand));
    }

    public ICommand SaveQuickFilterCommand { get; }

    public ICommand AddQuickFilterTagCommand { get; }
}