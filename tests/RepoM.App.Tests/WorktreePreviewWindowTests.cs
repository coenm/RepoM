namespace RepoM.App.Tests;

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using AwesomeAssertions;
using Meziantou.Xunit;
using Microsoft.Win32;
using RepoM.UI.Themes;
using RepoM.Plugin.WorktreePreview;
using VerifyTests;
using Xunit;

[DisableParallelization]
public class WorktreePreviewWindowTests
{
    [WpfFact]
    public void PreviewTags_ShouldRoundTripThroughXamlSerialization()
    {
        var model = new WorktreePreviewViewModel();
        foreach (var entry in model.Families.SelectMany(family => new[] { family.Primary }.Concat(family.Worktrees)))
        {
            var tags = new ItemsControl { ItemsSource = entry.Tags };
            string xaml = XamlWriter.Save(tags);
            var restored = (ItemsControl)XamlReader.Parse(xaml);

            restored.Items.Cast<string>().Should().Equal(entry.Tags);
        }
    }

    [WpfTheory]
    [InlineData(WorktreePreviewTheme.Dark)]
    [InlineData(WorktreePreviewTheme.Light)]
    public async Task Preview_ShouldMatchThemeSnapshots(WorktreePreviewTheme theme)
    {
        var window = new WorktreePreviewWindow();
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/RepoM.UI;component/i18n/en-us.xaml", UriKind.Relative),
        });

        ((ComboBox)window.FindName("ThemeSelector")).SelectedValue = theme;
        await VerifyPreviewAsync(window, theme);
    }

    private static async Task VerifyPreviewAsync(
        Window window,
        WorktreePreviewTheme theme,
        [CallerFilePath] string sourceFile = "")
    {
        // StaFact 1.x does not propagate Verify.Xunit's AsyncLocal test context from its Before hook.
        using var verifier = new InnerVerifier(
            Path.GetDirectoryName(sourceFile)!,
            $"{nameof(WorktreePreviewWindowTests)}.{nameof(Preview_ShouldMatchThemeSnapshots)}.{theme}");
        await verifier.Verify(window);
    }

    [WpfTheory]
    [InlineData(WorktreePreviewTheme.Dark)]
    [InlineData(WorktreePreviewTheme.Light)]
    public async Task Preview_ShouldFilterTagsAndKeepContextMenuActionsScopedToClickedEntry(WorktreePreviewTheme theme)
    {
        var window = new WorktreePreviewWindow();
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/RepoM.UI;component/i18n/en-us.xaml", UriKind.Relative),
        });
        ContextMenu? openMenu = null;

        try
        {
            window.Show();
            ((ComboBox)window.FindName("ThemeSelector")).SelectedValue = theme;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            var model = (WorktreePreviewViewModel)window.DataContext;
            var list = (ListBox)window.FindName("Repositories");
            WorktreePreviewEntry worktree = model.Families[0].Worktrees[0];
            var row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(worktree);
            var presenter = FindVisualChild<ContentPresenter>(row);
            var tags = (ItemsControl)list.ItemTemplate.FindName("Tags", presenter);
            var tagContainer = (ContentPresenter)tags.ItemContainerGenerator.ContainerFromItem("authentication");
            var tagButton = FindVisualChild<Button>(tagContainer);
            ((IInvokeProvider)new ButtonAutomationPeer(tagButton).GetPattern(PatternInterface.Invoke)).Invoke();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            model.Filter.Should().Be("authentication");
            list.Items.Cast<WorktreePreviewEntry>().Should().Equal(model.Families[0].Primary, worktree);

            list.SelectedItem = model.Families[0].Primary;
            openMenu = await OpenMenu(worktree);
            await AssertFlatPopupFrameAsync(openMenu, openMenu);
            list.SelectedItem.Should().BeSameAs(worktree);
            var favorite = openMenu.Items.OfType<MenuItem>().Single(item => item.Name == "WorktreePreviewFavorite");
            favorite.SetCurrentValue(MenuItem.IsCheckedProperty, true);
            worktree.IsFavorite.Should().BeTrue();

            var git = openMenu.Items.OfType<MenuItem>().Single(item => item.Name == "WorktreePreviewGit");
            await OpenSubmenuAsync(git);
            if (theme == WorktreePreviewTheme.Light)
            {
                ((SolidColorBrush)git.Foreground).Color.R.Should().BeLessThan(128);
            }
            else
            {
                ((SolidColorBrush)git.Foreground).Color.R.Should().BeGreaterThan(128);
            }

            Color menuBackground = ((SolidColorBrush)openMenu.Background).Color;
            (await GetSubmenuBackgroundAsync(git)).Should().Be(menuBackground);
            await AssertFlatPopupFrameAsync(git, openMenu);
            var checkout = git.Items.OfType<MenuItem>().Single(item => item.Name == "Checkout");
            await OpenSubmenuAsync(checkout);
            (await GetSubmenuBackgroundAsync(checkout)).Should().Be(menuBackground);
            await AssertFlatPopupFrameAsync(checkout, openMenu);
            checkout.IsSubmenuOpen = false;
            var fetch = git.Items.OfType<MenuItem>().Single(item => item.Name == "Fetch");
            fetch.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            var feedback = (TextBlock)window.FindName("PreviewFeedback");
            feedback.Text.Should().Contain("Fetch").And.Contain(worktree.Name).And.Contain("No action was executed");
            worktree.Branch.Should().Be("feature/login");

            git.IsSubmenuOpen = false;
            var primary = openMenu.Items.OfType<MenuItem>().Single(item => item.Name == "WorktreePreviewGoToPrimary");
            primary.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            openMenu.IsOpen = false;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            model.Filter.Should().BeEmpty();
            list.SelectedItem.Should().BeSameAs(model.Families[0].Primary);

            openMenu = await OpenMenu(model.Families[0].Primary);
            openMenu.Items.OfType<MenuItem>().Should().NotContain(item => item.Name == "WorktreePreviewGoToPrimary");
            var worktrees = openMenu.Items.OfType<MenuItem>().Single(item => item.Name == "WorktreePreviewOtherWorktrees");
            worktrees.IsSubmenuOpen = true;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            (await GetSubmenuBackgroundAsync(worktrees)).Should().Be(((SolidColorBrush)openMenu.Background).Color);
            await AssertFlatPopupFrameAsync(worktrees, openMenu);
            worktrees.Items.OfType<MenuItem>().Single(item => Equals(item.Header, worktree.Name))
                .RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));

            list.SelectedItem.Should().BeSameAs(worktree);

            async Task<ContextMenu> OpenMenu(WorktreePreviewEntry entry)
            {
                list.ScrollIntoView(entry);
                await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                var container = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(entry);
                ContextMenu menu = container.ContextMenu;
                menu.PlacementTarget = container;
                menu.IsOpen = true;
                await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                return menu;
            }
        }
        finally
        {
            if (openMenu != null)
            {
                openMenu.IsOpen = false;
            }

            window.Close();
        }
    }

    [WpfFact]
    public async Task Preview_ShouldCycleThreeLayoutsAndRestoreThemAfterFiltering()
    {
        var window = new WorktreePreviewWindow();
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/RepoM.UI;component/i18n/en-us.xaml", UriKind.Relative),
        });

        try
        {
            window.Show();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            var model = (WorktreePreviewViewModel)window.DataContext;
            model.EnableAnimations = false;
            var list = (ListBox)window.FindName("Repositories");
            var search = (TextBox)window.FindName("SearchBox");
            var item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);
            var presenter = FindVisualChild<ContentPresenter>(item);
            var button = (Button)list.ItemTemplate.FindName("DisplayModeButton", presenter);
            var details = (StackPanel)list.ItemTemplate.FindName("EntryDetails", presenter);
            var name = (TextBlock)list.ItemTemplate.FindName("EntryName", presenter);
            var favorite = FindVisualChild<ToggleButton>(item);
            var automation = (IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke);
            var extendedHeight = item.ActualHeight;
            Point titlePosition = name.TranslatePoint(new Point(), item);
            Point buttonPosition = button.TranslatePoint(new Point(), item);
            (titlePosition.X - buttonPosition.X - button.ActualWidth).Should().BeGreaterThanOrEqualTo(4);
            button.ActualWidth.Should().BeLessThan(favorite.ActualWidth);
            button.ActualHeight.Should().BeLessThan(favorite.ActualHeight);
            var titleFontSize = name.FontSize;
            var card = (Border)item.Template.FindName("card", item);
            Color restingCardColor = ((SolidColorBrush)card.Background).Color;
            list.SelectedItem = model.Families[0].Primary;
            Color selectedCardColor = ((SolidColorBrush)card.Background).Color;
            selectedCardColor.Should().NotBe(restingCardColor);
            ((SolidColorBrush)card.BorderBrush).Color.A.Should().Be(0);

            window.Activate();
            button.Focus().Should().BeTrue();
            button.IsKeyboardFocused.Should().BeTrue();
            var buttonSurface = (Border)button.Template.FindName("surface", button);
            buttonSurface.BorderThickness.Should().Be(new Thickness(0));
            ((SolidColorBrush)buttonSurface.Background).Color.A.Should().BeGreaterThan(0);
            ((SolidColorBrush)card.Background).Color.Should().NotBe(selectedCardColor);
            button.FocusVisualStyle.Should().BeNull();
            item.FocusVisualStyle.Should().BeNull();

            automation.Invoke();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            list.Items.Cast<WorktreePreviewEntry>().Should().NotContain(model.Families[0].Worktrees);
            model.Families[0].Primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Minimal);
            list.SelectedItem.Should().BeSameAs(model.Families[0].Primary);
            details.Visibility.Should().Be(Visibility.Collapsed);
            name.IsVisible.Should().BeTrue();
            favorite.IsVisible.Should().BeTrue();
            item.ActualHeight.Should().BeLessThan(extendedHeight / 2);
            name.TranslatePoint(new Point(), item).Should().Be(titlePosition);
            name.FontSize.Should().Be(titleFontSize);
            button.ToolTip.Should().Be("Show details");

            automation.Invoke();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            model.Families[0].Primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Extended);
            details.Visibility.Should().Be(Visibility.Visible);
            list.Items.Cast<WorktreePreviewEntry>().Should().NotContain(model.Families[0].Worktrees);
            button.ToolTip.Should().Be("Show details and worktrees");

            automation.Invoke();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            list.Items.Cast<WorktreePreviewEntry>().Should().Contain(model.Families[0].Worktrees);
            model.Families[0].Primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.ExtendedWithWorktrees);
            button.ToolTip.Should().Be("Show name and favorite only");

            var childItem = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(model.Families[0].Worktrees[0]);
            var childPresenter = FindVisualChild<ContentPresenter>(childItem);
            var childButton = (Button)list.ItemTemplate.FindName("DisplayModeButton", childPresenter);
            var childDetails = (StackPanel)list.ItemTemplate.FindName("EntryDetails", childPresenter);
            var childName = (TextBlock)list.ItemTemplate.FindName("EntryName", childPresenter);
            Point childTitlePosition = childName.TranslatePoint(new Point(), childItem);
            var childExtendedHeight = childItem.ActualHeight;
            var childAutomation = (IInvokeProvider)new ButtonAutomationPeer(childButton).GetPattern(PatternInterface.Invoke);
            var childCard = (Border)childItem.Template.FindName("card", childItem);
            var childEntry = (Border)list.ItemTemplate.FindName("entry", childPresenter);
            Color hierarchyColor = ((SolidColorBrush)childEntry.BorderBrush).Color;
            Thickness hierarchyThickness = childEntry.BorderThickness;
            Color restingChildColor = ((SolidColorBrush)childCard.Background).Color;
            list.SelectedItem = model.Families[0].Worktrees[0];
            ((SolidColorBrush)childCard.Background).Color.Should().NotBe(restingChildColor);
            ((SolidColorBrush)childCard.BorderBrush).Color.A.Should().Be(0);
            ((SolidColorBrush)childEntry.BorderBrush).Color.Should().Be(hierarchyColor);
            childEntry.BorderThickness.Should().Be(hierarchyThickness);
            childDetails.IsVisible.Should().BeTrue();
            childAutomation.Invoke();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            childDetails.Visibility.Should().Be(Visibility.Collapsed);
            childItem.ActualHeight.Should().BeLessThan(childExtendedHeight);
            childName.TranslatePoint(new Point(), childItem).Should().Be(childTitlePosition);
            model.Families[0].Worktrees[1].AreDetailsVisible.Should().BeTrue();

            automation.Invoke();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            search.SetCurrentValue(TextBox.TextProperty, "login");
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            list.Items.Cast<WorktreePreviewEntry>().Should().Equal(model.Families[0].Primary, model.Families[0].Worktrees[0]);
            item = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);
            presenter = FindVisualChild<ContentPresenter>(item);
            button = (Button)list.ItemTemplate.FindName("DisplayModeButton", presenter);
            button.IsEnabled.Should().BeFalse();
            model.Families[0].Primary.AreDetailsVisible.Should().BeTrue();
            model.Families[0].Worktrees[0].AreDetailsVisible.Should().BeTrue();
            window.Title.Should().Be("RepoM - Worktree preview");

            search.SetCurrentValue(TextBox.TextProperty, string.Empty);
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            model.Families[0].Primary.AreDetailsVisible.Should().BeFalse();
            model.Families[0].Primary.AreWorktreesVisible.Should().BeFalse();
            model.Families[0].Worktrees[0].AreDetailsVisible.Should().BeFalse();
            list.Items.Cast<WorktreePreviewEntry>().Should().NotContain(model.Families[0].Worktrees);
        }
        finally
        {
            window.Close();
        }
    }

    [WpfFact]
    public async Task Preview_ShouldSwitchThemesLocallyAndFollowWindowsWithoutLosingState()
    {
        var systemTheme = new TestSystemTheme();
        var window = new WorktreePreviewWindow(systemTheme);
        var otherWindow = new WorktreePreviewWindow(new TestSystemTheme());
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/RepoM.UI;component/i18n/en-us.xaml", UriKind.Relative),
        });
        ContextMenu? menu = null;

        try
        {
            window.Show();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            systemTheme.SubscriberCount.Should().Be(1);
            var selector = (ComboBox)window.FindName("ThemeSelector");
            var model = (WorktreePreviewViewModel)window.DataContext;
            var list = (ListBox)window.FindName("Repositories");
            var search = (TextBox)window.FindName("SearchBox");
            WorktreePreviewEntry primary = model.Families[0].Primary;
            WorktreePreviewEntry worktree = model.Families[0].Worktrees[0];
            primary.DisplayMode = WorktreePreviewDisplayMode.Minimal;
            worktree.DisplayMode = WorktreePreviewDisplayMode.Minimal;
            worktree.IsFavorite = true;
            model.Filter = "login";
            list.SelectedItem = worktree;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            var row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(worktree);
            var card = (Border)row.Template.FindName("card", row);
            var presenter = FindVisualChild<ContentPresenter>(row);
            var tags = (ItemsControl)list.ItemTemplate.FindName("Tags", presenter);
            var tag = FindVisualChild<Button>(tags.ItemContainerGenerator.ContainerFromIndex(0));
            var darkBackground = ((SolidColorBrush)window.Background).Color;
            var darkCard = ((SolidColorBrush)card.Background).Color;
            var darkTag = ((SolidColorBrush)tag.Foreground).Color;
            var darkSearch = ((SolidColorBrush)search.Foreground).Color;

            selector.SelectedValue = WorktreePreviewTheme.Light;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            var lightBackground = ((SolidColorBrush)window.Background).Color;
            lightBackground.Should().NotBe(darkBackground);
            ((SolidColorBrush)card.Background).Color.Should().NotBe(darkCard);
            ((SolidColorBrush)tag.Foreground).Color.Should().NotBe(darkTag);
            ((SolidColorBrush)search.Foreground).Color.Should().NotBe(darkSearch);
            ((SolidColorBrush)otherWindow.Background).Color.Should().Be(darkBackground);
            list.ItemContainerGenerator.ContainerFromItem(worktree).Should().BeSameAs(row);
            list.SelectedItem.Should().BeSameAs(worktree);
            model.Filter.Should().Be("login");
            worktree.IsFavorite.Should().BeTrue();
            primary.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Minimal);
            worktree.DisplayMode.Should().Be(WorktreePreviewDisplayMode.Minimal);

            menu = row.ContextMenu;
            menu.PlacementTarget = row;
            menu.IsOpen = true;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            var lightMenu = ((SolidColorBrush)menu.Background).Color;
            var git = menu.Items.OfType<MenuItem>().Single(item => item.Name == "WorktreePreviewGit");
            git.IsSubmenuOpen = true;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            (await GetSubmenuBackgroundAsync(git)).Should().Be(lightMenu);

            selector.SelectedValue = WorktreePreviewTheme.System;
            await Task.Run(() => systemTheme.Change(false));
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            ((SolidColorBrush)window.Background).Color.Should().Be(darkBackground);
            menu.IsOpen.Should().BeFalse();
            menu.IsOpen = true;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            ((SolidColorBrush)menu.Background).Color.Should().NotBe(lightMenu);
            git = menu.Items.OfType<MenuItem>().Single(item => item.Name == "WorktreePreviewGit");
            git.IsSubmenuOpen = true;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            (await GetSubmenuBackgroundAsync(git)).Should().Be(((SolidColorBrush)menu.Background).Color);

            await Task.Run(() => systemTheme.Change(true));
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            ((SolidColorBrush)window.Background).Color.Should().Be(lightBackground);
            menu.IsOpen.Should().BeFalse();

            selector.SelectedValue = WorktreePreviewTheme.Dark;
            await Task.Run(() => systemTheme.Change(true));
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            ((SolidColorBrush)window.Background).Color.Should().Be(darkBackground);
            selector.SelectedValue = WorktreePreviewTheme.System;
            ((SolidColorBrush)window.Background).Color.Should().Be(lightBackground);
            model.Filter.Should().Be("login");
            list.SelectedItem.Should().BeSameAs(worktree);
            worktree.IsFavorite.Should().BeTrue();
            model.Filter = string.Empty;
            primary.AreDetailsVisible.Should().BeFalse();
            worktree.AreDetailsVisible.Should().BeFalse();
        }
        finally
        {
            if (menu != null)
            {
                menu.IsOpen = false;
            }

            window.Close();
            otherWindow.Close();
        }

        systemTheme.SubscriberCount.Should().Be(0);
    }

    [WpfFact]
    public async Task Preview_ShouldReportWindowsThemeReadFailuresAndRecoverOnNextChange()
    {
        var systemTheme = new TestSystemTheme { IsReadUnavailable = true };
        var window = new WorktreePreviewWindow(systemTheme);
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/RepoM.UI;component/i18n/en-us.xaml", UriKind.Relative),
        });

        try
        {
            window.Show();
            var selector = (ComboBox)window.FindName("ThemeSelector");
            selector.SelectedValue = WorktreePreviewTheme.Light;
            var lastBackground = ((SolidColorBrush)window.Background).Color;
            selector.SelectedValue = WorktreePreviewTheme.System;

            var feedback = (TextBlock)window.FindName("PreviewFeedback");
            feedback.Visibility.Should().Be(Visibility.Visible);
            feedback.Text.Should().Contain("Could not read the Windows app theme").And.Contain("Theme unavailable");
            ((SolidColorBrush)window.Background).Color.Should().Be(lastBackground);

            systemTheme.IsReadUnavailable = false;
            systemTheme.Change(false);
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            ((SolidColorBrush)window.Background).Color.Should().NotBe(lastBackground);
            feedback.Visibility.Should().Be(Visibility.Collapsed);
        }
        finally
        {
            window.Close();
        }

        systemTheme.SubscriberCount.Should().Be(0);
    }

    [WpfTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Preview_ShouldAnimateExpansionWithoutScalingTitleAndRestoreAutomaticHeight(bool isWorktree)
    {
        var window = new WorktreePreviewWindow(new TestSystemTheme());
        try
        {
            window.Show();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            var model = (WorktreePreviewViewModel)window.DataContext;
            var list = (ListBox)window.FindName("Repositories");
            var entry = isWorktree ? model.Families[0].Worktrees[0] : model.Families[0].Primary;
            var row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(entry);
            double expandedHeight = row.ActualHeight;
            entry.DisplayMode = WorktreePreviewDisplayMode.Minimal;
            list.UpdateLayout();
            double minimalHeight = row.ActualHeight;
            var presenter = FindVisualChild<ContentPresenter>(row);
            var button = (Button)list.ItemTemplate.FindName("DisplayModeButton", presenter);
            var title = (TextBlock)list.ItemTemplate.FindName("EntryName", presenter);
            Point titlePosition = title.TranslatePoint(new Point(), row);
            double fontSize = title.FontSize;
            list.SelectedItem = entry;

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            list.UpdateLayout();

            row.HasAnimatedProperties.Should().BeTrue();
            row.ActualHeight.Should().BeApproximately(minimalHeight, 1);
            title.TranslatePoint(new Point(), row).Should().Be(titlePosition);
            title.FontSize.Should().Be(fontSize);
            await Task.Delay(80);
            list.UpdateLayout();
            row.ActualHeight.Should().BeGreaterThan(minimalHeight).And.BeLessThanOrEqualTo(expandedHeight);

            await Task.Delay(250);
            list.UpdateLayout();
            row.HasAnimatedProperties.Should().BeFalse();
            double.IsNaN(row.Height).Should().BeTrue();
            row.ActualHeight.Should().BeApproximately(expandedHeight, 0.1);
            row.Opacity.Should().Be(1);
            list.SelectedItem.Should().BeSameAs(entry);

            // Interrupt repeated transitions, then disable them through the actual preview control.
            for (var index = 0; index < 5; index++)
            {
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }

            var toggle = (CheckBox)window.FindName("AnimationsToggle");
            toggle.SetCurrentValue(ToggleButton.IsCheckedProperty, false);
            list.UpdateLayout();
            model.EnableAnimations.Should().BeFalse();
            row.HasAnimatedProperties.Should().BeFalse();
            double.IsNaN(row.Height).Should().BeTrue();

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            list.UpdateLayout();
            row.HasAnimatedProperties.Should().BeFalse();
            row.ActualHeight.Should().BeApproximately(entry.AreDetailsVisible ? expandedHeight : minimalHeight, 0.1);

            toggle.SetCurrentValue(ToggleButton.IsCheckedProperty, true);
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            window.Width += 30;
            window.UpdateLayout();
            row.HasAnimatedProperties.Should().BeFalse();
            double.IsNaN(row.Height).Should().BeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [WpfFact]
    public async Task Preview_ShouldAnimateRevealedWorktreesAndClearAnimationWhenFilteringOrRecycling()
    {
        var window = new WorktreePreviewWindow(new TestSystemTheme());
        try
        {
            var model = (WorktreePreviewViewModel)window.DataContext;
            WorktreePreviewEntry primary = model.Families[0].Primary;
            primary.DisplayMode = WorktreePreviewDisplayMode.Extended;
            window.Show();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            var list = (ListBox)window.FindName("Repositories");
            var row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(primary);
            var presenter = FindVisualChild<ContentPresenter>(row);
            var button = (Button)list.ItemTemplate.FindName("DisplayModeButton", presenter);
            list.SelectedItem = primary;

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            list.UpdateLayout();

            var children = model.Families[0].Worktrees
                .Select(child => (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(child)).ToArray();
            foreach (ListBoxItem child in children)
            {
                child.HasAnimatedProperties.Should().BeTrue();
                child.ActualHeight.Should().BeLessThan(1);
                child.Opacity.Should().BeLessThan(1);
            }

            await Task.Delay(80);
            list.UpdateLayout();
            foreach (ListBoxItem child in children)
            {
                child.ActualHeight.Should().BeGreaterThan(0);
                child.Opacity.Should().BeGreaterThan(0);
            }

            await Task.Delay(250);
            list.UpdateLayout();
            foreach (ListBoxItem child in children)
            {
                child.HasAnimatedProperties.Should().BeFalse();
                double.IsNaN(child.Height).Should().BeTrue();
                child.Opacity.Should().Be(1);
            }

            list.SelectedItem.Should().BeSameAs(primary);
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); // Minimal.
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); // Details.
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); // Worktrees.
            var interruptedChild = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(model.Families[0].Worktrees[0]);
            interruptedChild.HasAnimatedProperties.Should().BeTrue();
            model.Filter = "retry-policy";
            list.UpdateLayout();
            interruptedChild.HasAnimatedProperties.Should().BeFalse();
            row.HasAnimatedProperties.Should().BeFalse();
            model.Filter = string.Empty;
            list.UpdateLayout();
            foreach (WorktreePreviewEntry entry in model.VisibleEntries)
            {
                if (list.ItemContainerGenerator.ContainerFromItem(entry) is ListBoxItem visibleRow)
                {
                    visibleRow.HasAnimatedProperties.Should().BeFalse();
                    double.IsNaN(visibleRow.Height).Should().BeTrue();
                    visibleRow.Opacity.Should().Be(1);
                }
            }

            model.EnableAnimations = false;
            row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(primary);
            presenter = FindVisualChild<ContentPresenter>(row);
            button = (Button)list.ItemTemplate.FindName("DisplayModeButton", presenter);
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            list.UpdateLayout();
            foreach (WorktreePreviewEntry entry in model.Families[0].Worktrees)
            {
                var child = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(entry);
                child.HasAnimatedProperties.Should().BeFalse();
                child.ActualHeight.Should().BeGreaterThan(0);
                child.Opacity.Should().Be(1);
            }
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class TestSystemTheme : IPreviewSystemTheme
    {
        private bool _isLight = true;

        public event UserPreferenceChangedEventHandler? Changed;

        public bool IsReadUnavailable { get; set; }

        public bool IsLight => IsReadUnavailable ? throw new IOException("Theme unavailable") : _isLight;

        public int SubscriberCount => Changed?.GetInvocationList().Length ?? 0;

        public void Change(bool isLight)
        {
            _isLight = isLight;
            Changed?.Invoke(this, new UserPreferenceChangedEventArgs(UserPreferenceCategory.General));
        }
    }

    private static async Task AssertFlatPopupFrameAsync(ItemsControl menu, ContextMenu root)
    {
        // Measure after the theme's popup slide animation has finished.
        await Task.Delay(400);
        await menu.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        DependencyObject? parent = VisualTreeHelper.GetParent(menu.ItemContainerGenerator.ContainerFromIndex(0));
        FrameworkElement? popupRoot = null;
        Border? surface = null;
        var hasSurface = false;
        while (parent != null)
        {
            if (parent is FrameworkElement element)
            {
                popupRoot = element;
                element.Margin.Should().Be(new Thickness(0));
                element.Effect.Should().BeNull();
            }

            if (parent is Border border)
            {
                border.BorderThickness.Should().Be(hasSurface ? new Thickness(0) : root.BorderThickness);
                if (!hasSurface)
                {
                    surface = border;
                    ((SolidColorBrush)border.BorderBrush).Color.Should().Be(((SolidColorBrush)root.BorderBrush).Color);
                    hasSurface = true;
                }
            }

            parent = VisualTreeHelper.GetParent(parent);
        }

        hasSurface.Should().BeTrue();
        surface!.TranslatePoint(new Point(), popupRoot).Should().Be(new Point());
        surface.ActualWidth.Should().BeApproximately(popupRoot!.ActualWidth, 0.1);
        surface.ActualHeight.Should().BeApproximately(popupRoot.ActualHeight, 0.1);
    }

    private static async Task<Color> GetSubmenuBackgroundAsync(MenuItem menu)
    {
        DependencyObject? container = menu.ItemContainerGenerator.ContainerFromIndex(0);
        for (int attempt = 0; container is null && attempt < 40; attempt++)
        {
            await Task.Delay(25);
            await menu.Dispatcher.InvokeAsync(menu.UpdateLayout, DispatcherPriority.Render);
            container = menu.ItemContainerGenerator.ContainerFromIndex(0);
        }

        container.Should().NotBeNull($"submenu items for '{menu.Header}' should be generated when it is open");
        DependencyObject? parent = VisualTreeHelper.GetParent(container!);
        while (parent != null)
        {
            if (parent is Border { Background: SolidColorBrush brush } && brush.Color.A > 0)
            {
                return brush.Color;
            }

            parent = VisualTreeHelper.GetParent(parent);
        }

        throw new InvalidOperationException("No visible submenu background found.");
    }

    private static async Task OpenSubmenuAsync(MenuItem menu)
    {
        if (menu.IsSubmenuOpen)
        {
            return;
        }

        var opened = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        RoutedEventHandler handler = (_, _) => opened.TrySetResult();
        menu.SubmenuOpened += handler;
        try
        {
            menu.IsSubmenuOpen = true;
            await opened.Task.WaitAsync(TimeSpan.FromSeconds(2));
            await menu.Dispatcher.InvokeAsync(menu.UpdateLayout, DispatcherPriority.Render);
        }
        finally
        {
            menu.SubmenuOpened -= handler;
        }
    }

    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        return TryFindVisualChild<T>(parent) ?? throw new InvalidOperationException($"Could not find {typeof(T).Name}.");
    }

    private static T? TryFindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T result)
            {
                return result;
            }

            if (TryFindVisualChild<T>(child) is T descendant)
            {
                return descendant;
            }
        }

        return null;
    }
}
