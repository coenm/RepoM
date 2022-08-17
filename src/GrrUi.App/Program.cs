namespace GrrUi.App;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GrrUi.App.Model;
using GrrUi.App.UI;
using NStack;
using RepoM.Ipc;
using Terminal.Gui;

static class Program
{
    private const int BUTTON_BORDER = 4; // 2 chars to the left, 2 to the right
    private const int BUTTON_DISTANCE = 1;

    private static IpcClient _client = null!;
    private static ListView? _repositoryList;
    private static RepositoriesView? _repositoriesView;
    private static TextField? _filterField;

    static void Main(string[] args)
    {
        _client = new IpcClient(new DefaultIpcEndpoint());
        IpcClient.Result answer = _client.GetRepositories();

        var repositoryCount = answer.Repositories.Length;
        if (repositoryCount == 0)
        {
            if (!string.IsNullOrEmpty(answer?.Answer))
            {
                Console.WriteLine(answer.Answer);
            }
            else
            {
                Console.WriteLine("No repositories yet");
            }

            return;
        }

        _repositoriesView = new RepositoriesView(answer.Repositories);

        Application.Init();

        var filterLabel = new Label(1, 1, "Filter: ");
        _filterField = new TextField(string.Empty)
            {
                X = Pos.Right(filterLabel) + 2,
                Y = Pos.Top(filterLabel),
                Width = Dim.Fill(margin: 1),
            };
        _filterField.TextChanged += FilterFieldOnTextChanged;

        _repositoryList = new ListView(_repositoriesView.Repositories)
            {
                X = Pos.Left(filterLabel),
                Y = Pos.Bottom(filterLabel) + 1,
                Width = Dim.Fill(margin: 1),
                Height = Dim.Fill() - 2,
            };

        var win = new KeyPreviewWindow("grr: Git repositories of RepoM");
        win.Add(filterLabel);
        win.Add(_filterField);
        win.Add(_repositoryList);
            // {
            //     filterLabel,
            //     _filterField,
            //     _repositoryList,
            // };

        var buttonX = Pos.Left(filterLabel);

        var navigationButton = new Button("Navigate")
            {
                X = buttonX,
                Y = Pos.AnchorEnd(1),
                CanFocus = false,
            };
        if (CanNavigate)
        {
            navigationButton.Clicked += Navigate;
        }
        else
        {
            navigationButton.Clicked += CopyNavigationCommandAndQuit;
        }

        buttonX = buttonX + navigationButton.Text.Length + BUTTON_BORDER + BUTTON_DISTANCE;
        var copyPathButton = new Button("Copy")
            {
                X = buttonX,
                Y = Pos.AnchorEnd(1),
                CanFocus = false,
            };
        copyPathButton.Clicked += Copy;

        buttonX = buttonX + copyPathButton.Text.Length + BUTTON_BORDER + BUTTON_DISTANCE;
        var browseButton = new Button("Browse")
            {
                
                X = buttonX,
                Y = Pos.AnchorEnd(1),
                CanFocus = false,
            };
        browseButton.Clicked += Browse;

        var quitButton = new Button("Quit")
            {
                X = Pos.AnchorEnd("Quit".Length + BUTTON_BORDER + BUTTON_DISTANCE),
                Y = Pos.AnchorEnd(1),
                CanFocus = false,
            };
        quitButton.Clicked += QuitButtonOnClicked;

        win.Add(navigationButton, copyPathButton, browseButton, quitButton);

        win.DefineKeyAction(Key.Enter, () => _repositoryList.SetFocus());
        win.DefineKeyAction(Key.Esc, () =>
            {
                if (_filterField.HasFocus)
                {
                    SetFilterText(string.Empty);
                }
                else
                {
                    _filterField.SetFocus();
                }
            });

        if (args.Length > 0)
        {
            SetFilterText(string.Join(" ", args));
        }

        Application.Top.Add(win);
        Application.Run();
    }

    private static void QuitButtonOnClicked()
    {
        Application.RequestStop();
    }
    
    private static void SetFilterText(string filter)
    {
        if (_filterField == null)
        {
            return;
        }

        _filterField.Text = filter;
        _filterField.PositionCursor();
        FilterField_Changed(_filterField, NStack.ustring.Empty);
    }

    private static void Navigate()
    {
        ExecuteOnSelectedRepository(r =>
            {
                var command = $"cd \"{r.SafePath}\"";

                // type the path into the console which is hosting grrui.exe to change to the directory
                TextCopy.ClipboardService.SetText(command);
                Grr.App.ConsoleExtensions.WriteConsoleInput(Process.GetCurrentProcess(), command, waitMilliseconds: 1000);

                Application.RequestStop();
            });
    }

    private static void CopyNavigationCommandAndQuit()
    {
        ExecuteOnSelectedRepository(r =>
            {
                var command = $"cd \"{r.SafePath}\"";
                TextCopy.ClipboardService.SetText(command);
                TimelyMessage.ShowMessage("Copied to clipboard. Please paste and run the command manually now.", TimeSpan.FromMilliseconds(1000));
                Application.RequestStop();
            });
    }

    private static void Copy()
    {
        ExecuteOnSelectedRepository(r =>
            {
                var command = $"\"{r.SafePath}\"";
                TextCopy.ClipboardService.SetText(command);
            });
    }

    private static void Browse()
    {
        ExecuteOnSelectedRepository(r => Process.Start(new ProcessStartInfo(r.SafePath) { UseShellExecute = true, }));
    }

    private static void ExecuteOnSelectedRepository(Action<Repository> action)
    {
        RepositoryView[]? repositories = _repositoriesView?.Repositories;

        if (repositories == null || _repositoryList == null)
        {
            return;
        }

        if (!(repositories.Length > _repositoryList.SelectedItem))
        {
            return;
        }

        RepositoryView current = repositories[_repositoryList.SelectedItem];
        action(current.Repository);
    }

    private static void FilterFieldOnTextChanged(ustring obj)
    {
        FilterField_Changed(_filterField, obj);
    }

    private static void FilterField_Changed(object? sender, NStack.ustring e)
    {
        if (_repositoriesView == null)
        {
            return;
        }

        _repositoriesView.Filter = (sender as TextField)?.Text?.ToString() ?? string.Empty;
        _repositoryList?.SetSource(_repositoriesView.Repositories);
    }

    private static bool CanNavigate => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}