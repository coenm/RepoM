namespace RepoM.App;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using RepoM.App.Controls;
using RepoM.App.Themes;
using RepoM.App.ViewModels;

public partial class WorktreePreviewWindow : Window
{
    private readonly WorktreePreviewViewModel _viewModel = new();
    private readonly HashSet<ListBoxItem> _animatedRows = [];
    private readonly IPreviewSystemTheme _systemTheme;
    private readonly ResourceDictionary _themeResources;
    private WorktreePreviewTheme _effectiveTheme = WorktreePreviewTheme.Dark;
    private AcrylicContextMenu? _openMenu;
    private string? _themeError;
    private bool _isListening;
    private bool _isClosed;

    public WorktreePreviewWindow()
        : this(new WindowsPreviewSystemTheme())
    {
    }

    public WorktreePreviewWindow(IPreviewSystemTheme systemTheme)
    {
        ArgumentNullException.ThrowIfNull(systemTheme);
        _systemTheme = systemTheme;
        InitializeComponent();
        _themeResources = Resources.MergedDictionaries[1];
        DataContext = _viewModel;
        _viewModel.PropertyChanged += PreviewSettings_Changed;
        ThemeSelector.SelectionChanged += (_, _) => ApplySelectedTheme();
        Loaded += Window_Loaded;
        SizeChanged += (_, _) => FinishRowAnimations();
    }

    protected override void OnClosed(EventArgs e)
    {
        _isClosed = true;
        FinishRowAnimations();
        _viewModel.PropertyChanged -= PreviewSettings_Changed;
        if (_isListening)
        {
            _systemTheme.Changed -= WindowsTheme_Changed;
            _isListening = false;
        }

        if (_openMenu != null)
        {
            _openMenu.IsOpen = false;
        }

        base.OnClosed(e);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_isListening)
        {
            _systemTheme.Changed += WindowsTheme_Changed;
            _isListening = true;
        }

        ApplySelectedTheme();
        SearchBox.Focus();
    }

    private void WindowsTheme_Changed(object sender, UserPreferenceChangedEventArgs e)
    {
        _ = Dispatcher.InvokeAsync(() =>
        {
            if (!_isClosed && Equals(ThemeSelector.SelectedValue, WorktreePreviewTheme.System))
            {
                ApplySelectedTheme();
            }
        });
    }

    private void ApplySelectedTheme()
    {
        bool isLight;
        try
        {
            isLight = ThemeSelector.SelectedValue switch
            {
                WorktreePreviewTheme.Dark => false,
                WorktreePreviewTheme.Light => true,
                WorktreePreviewTheme.System => _systemTheme.IsLight,
                _ => throw new InvalidOperationException("Unknown preview theme."),
            };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or SecurityException)
        {
            Trace.TraceError("Cannot read the Windows app theme: {0}", exception);
            _themeError = string.Format(Text("WorktreePreviewThemeError"), exception.Message);
            PreviewFeedback.Text = _themeError;
            PreviewFeedback.Visibility = Visibility.Visible;
            return;
        }

        if (_themeError != null)
        {
            if (PreviewFeedback.Text == _themeError)
            {
                PreviewFeedback.Visibility = Visibility.Collapsed;
            }

            _themeError = null;
        }

        var theme = isLight ? WorktreePreviewTheme.Light : WorktreePreviewTheme.Dark;
        if (theme == _effectiveTheme)
        {
            return;
        }

        // Acrylic popup frames hold brush references; recreate them on the next opening.
        if (_openMenu != null)
        {
            _openMenu.IsOpen = false;
        }

        _themeResources.Source = new Uri($"/RepoM;component/Themes/WorktreePreview.{theme}.xaml", UriKind.Relative);
        _effectiveTheme = theme;
    }

    private void ClearFilter_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Filter = string.Empty;
        SearchBox.Focus();
    }

    private void Tag_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Filter = (string)((Button)sender).Content;
        SearchBox.Focus();
    }

    private void DisplayMode_Click(object sender, RoutedEventArgs e)
    {
        var entry = (WorktreePreviewEntry)((Button)sender).DataContext;
        if (!_viewModel.EnableAnimations)
        {
            entry.CycleDisplayMode();
            return;
        }

        var previousEntries = _viewModel.VisibleEntries.ToHashSet();
        var row = (ListBoxItem)Repositories.ItemContainerGenerator.ContainerFromItem(entry);
        double previousHeight = row.ActualHeight;
        FinishRowAnimations();
        entry.CycleDisplayMode();
        Repositories.UpdateLayout();

        AnimateRow(row, previousHeight, false);
        foreach (WorktreePreviewEntry added in _viewModel.VisibleEntries.Where(item => !previousEntries.Contains(item)))
        {
            if (Repositories.ItemContainerGenerator.ContainerFromItem(added) is ListBoxItem child)
            {
                AnimateRow(child, 0, true);
            }
        }
    }

    private void PreviewSettings_Changed(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorktreePreviewViewModel.Filter)
            || (e.PropertyName == nameof(WorktreePreviewViewModel.EnableAnimations) && !_viewModel.EnableAnimations))
        {
            FinishRowAnimations();
        }
    }

    private void AnimateRow(ListBoxItem row, double fromHeight, bool fadeIn)
    {
        double targetHeight = row.ActualHeight;
        if (Math.Abs(fromHeight - targetHeight) < 0.1)
        {
            return;
        }

        var duration = TimeSpan.FromMilliseconds(180);
        var height = new DoubleAnimation(fromHeight, targetHeight, duration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        height.Completed += (_, _) => FinishRowAnimation(row);
        _animatedRows.Add(row);
        row.Unloaded += AnimatedRow_Unloaded;
        row.DataContextChanged += AnimatedRow_DataContextChanged;
        row.Height = fromHeight;
        row.BeginAnimation(HeightProperty, height);
        if (fadeIn)
        {
            row.Opacity = 0;
            row.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration));
        }
    }

    private void AnimatedRow_Unloaded(object sender, RoutedEventArgs e) => FinishRowAnimation((ListBoxItem)sender);

    private void AnimatedRow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) => FinishRowAnimation((ListBoxItem)sender);

    private void FinishRowAnimations()
    {
        foreach (ListBoxItem row in _animatedRows.ToArray())
        {
            FinishRowAnimation(row);
        }
    }

    private void FinishRowAnimation(ListBoxItem row)
    {
        if (!_animatedRows.Remove(row))
        {
            return;
        }

        row.Unloaded -= AnimatedRow_Unloaded;
        row.DataContextChanged -= AnimatedRow_DataContextChanged;
        // Return to automatic sizing, including after filtering or container recycling.
        row.BeginAnimation(HeightProperty, null);
        row.BeginAnimation(OpacityProperty, null);
        row.ClearValue(HeightProperty);
        row.ClearValue(OpacityProperty);
    }

    private void RepositoryMenu_Opened(object sender, RoutedEventArgs e)
    {
        var menu = (AcrylicContextMenu)sender;
        _openMenu = menu;
        if (!menu.Resources.MergedDictionaries.Contains(_themeResources))
        {
            // Popups have a separate resource scope from the window.
            menu.Resources.MergedDictionaries.Add(_themeResources);
        }

        var entry = (WorktreePreviewEntry)((FrameworkElement)menu.PlacementTarget).DataContext;
        Repositories.SelectedItem = entry;
        menu.Items.Clear();

        var heading = new StackPanel();
        heading.Children.Add(new TextBlock { Text = entry.Name, FontWeight = FontWeights.SemiBold });
        heading.Children.Add(new TextBlock { Text = entry.Branch, FontSize = 12 });
        menu.Items.Add(new AcrylicMenuItem { Header = heading, IsEnabled = false, ToolTip = entry.Path });
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateDemoAction("WorktreePreviewEditor", entry));
        menu.Items.Add(CreateDemoAction("WorktreePreviewTerminal", entry));
        menu.Items.Add(CreateDemoAction("WorktreePreviewExplorer", entry));
        menu.Items.Add(new Separator());

        WorktreePreviewFamily family = _viewModel.Families.Single(f => f.Primary == entry || f.Worktrees.Contains(entry));
        var git = CreateMenuItem("WorktreePreviewGit");
        git.Items.Add(CreateDemoAction("Fetch", entry));
        git.Items.Add(CreateDemoAction("Pull", entry));
        git.Items.Add(CreateDemoAction("Push", entry));
        git.Items.Add(new Separator());
        var checkout = CreateMenuItem("Checkout");
        foreach (WorktreePreviewEntry member in new[] { family.Primary }.Concat(family.Worktrees))
        {
            checkout.Items.Add(new AcrylicMenuItem
            {
                Header = member == entry
                    ? member.Branch
                    : string.Format(Text("WorktreePreviewBranchInUse"), member.Branch, member.Name),
                IsCheckable = true,
                IsChecked = member == entry,
                IsEnabled = false,
            });
        }

        var demoBranch = new AcrylicMenuItem { Header = "feature/demo" };
        demoBranch.Click += (_, _) => ShowDemoFeedback(entry, Text("Checkout") + " feature/demo");
        checkout.Items.Add(demoBranch);
        git.Items.Add(checkout);
        menu.Items.Add(git);

        if (entry.IsWorktree)
        {
            var primary = CreateMenuItem("WorktreePreviewGoToPrimary");
            primary.Click += (_, _) => NavigateTo(family.Primary, family);
            menu.Items.Add(primary);
        }

        if (family.Worktrees.Count > 0)
        {
            var worktrees = CreateMenuItem("WorktreePreviewOtherWorktrees");
            foreach (WorktreePreviewEntry worktree in family.Worktrees)
            {
                var target = new AcrylicMenuItem
                {
                    Header = worktree.Name,
                    InputGestureText = worktree.Branch,
                    IsEnabled = worktree != entry,
                };
                target.Click += (_, _) => NavigateTo(worktree, family);
                worktrees.Items.Add(target);
            }

            menu.Items.Add(worktrees);
        }

        menu.Items.Add(new Separator());
        var favorite = CreateMenuItem("WorktreePreviewFavorite");
        favorite.IsCheckable = true;
        favorite.SetBinding(MenuItem.IsCheckedProperty, new Binding(nameof(WorktreePreviewEntry.IsFavorite))
        {
            Source = entry,
            Mode = BindingMode.TwoWay,
        });
        menu.Items.Add(favorite);
        menu.Items.Add(new Separator());
        menu.Items.Add(new AcrylicMenuItem { Header = Text("WorktreePreviewMenuNotice"), IsEnabled = false });
    }

    private void RepositoryMenu_Closed(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(_openMenu, sender))
        {
            _openMenu = null;
        }
    }

    private AcrylicMenuItem CreateMenuItem(string key)
    {
        return new AcrylicMenuItem { Name = key, Header = Text(key) };
    }

    private AcrylicMenuItem CreateDemoAction(string key, WorktreePreviewEntry entry)
    {
        AcrylicMenuItem item = CreateMenuItem(key);
        item.Click += (_, _) => ShowDemoFeedback(entry, Text(key));
        return item;
    }

    private void ShowDemoFeedback(WorktreePreviewEntry entry, string action)
    {
        PreviewFeedback.Text = string.Format(Text("WorktreePreviewActionFeedback"), action, entry.Name);
        PreviewFeedback.Visibility = Visibility.Visible;
    }

    private void NavigateTo(WorktreePreviewEntry entry, WorktreePreviewFamily family)
    {
        _viewModel.Filter = string.Empty;
        family.Primary.DisplayMode = family.Primary.HasWorktrees
            ? WorktreePreviewDisplayMode.ExtendedWithWorktrees
            : WorktreePreviewDisplayMode.Extended;
        Repositories.SelectedItem = entry;
        Repositories.ScrollIntoView(entry);
    }

    private string Text(string key) => (string)FindResource(key);
}
