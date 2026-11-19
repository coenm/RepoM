namespace RepoM.App.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Input;
using AwesomeAssertions;
using FakeItEasy;
using Meziantou.Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.Api.Common;
using RepoM.Api.Git;
using RepoM.Api.QuickFilter;
using RepoM.App.Controls;
using RepoM.App.Plugins;
using RepoM.App.RepositoryActions;
using RepoM.App.RepositoryFiltering;
using RepoM.App.RepositoryOrdering;
using RepoM.App.Services;
using RepoM.App.Tests.ViewModels;
using RepoM.App.Themes;
using RepoM.App.ViewModels;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryFiltering;
using RepoM.Core.Plugin.RepositoryFiltering.Clause;
using RepoM.Core.Repositories.Favorite;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Store;
using RepoM.App.Tests.TestFramework;
using Xunit;

[DisableParallelization]
public class RepositoryFrontendTests
{
    [WpfFact]
    public async Task XamlPngComparer_ShouldAllowSmallRenderingNoiseButRejectVisibleChanges()
    {
        byte[] referencePixels = Enumerable.Repeat((byte)128, 100 * 100 * 4).ToArray();
        for (int offset = 3; offset < referencePixels.Length; offset += 4)
        {
            referencePixels[offset] = 255;
        }

        byte[] slightlyDifferentPixels = (byte[])referencePixels.Clone();
        slightlyDifferentPixels[0] = 130;
        var reference = CreatePng(100, 100, referencePixels);
        var slightlyDifferent = CreatePng(100, 100, slightlyDifferentPixels);
        (await XamlPngComparer.CompareAsync(slightlyDifferent, reference, new Dictionary<string, object>()))
            .IsEqual.Should().BeTrue();

        byte[] visiblyDifferentPixels = (byte[])referencePixels.Clone();
        for (int offset = 0; offset < visiblyDifferentPixels.Length / 2; offset += 4)
        {
            visiblyDifferentPixels[offset] = 0;
        }

        var visiblyDifferent = CreatePng(100, 100, visiblyDifferentPixels);
        (await XamlPngComparer.CompareAsync(visiblyDifferent, reference, new Dictionary<string, object>()))
            .IsEqual.Should().BeFalse();
    }

    [WpfTheory]
    [InlineData("Dark")]
    [InlineData("Light")]
    public void RepositoryScrollBar_ShouldUseCompactThemeAwareThumb(string theme)
    {
        var scrollbar = new ScrollBar { Orientation = Orientation.Vertical };
        scrollbar.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri($"/RepoM;component/Themes/WorktreePreview.{theme}.xaml", UriKind.Relative),
        });
        scrollbar.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/RepoM;component/Themes/RepositoryStyles.xaml", UriKind.Relative),
        });
        scrollbar.Style = (Style)scrollbar.FindResource("RepositoryScrollBar");
        scrollbar.Measure(new System.Windows.Size(12, 100));
        scrollbar.Arrange(new Rect(0, 0, 12, 100));
        scrollbar.UpdateLayout();
        scrollbar.ApplyTemplate();

        scrollbar.Width.Should().Be(12);
        var track = (Track)scrollbar.Template.FindName("PART_Track", scrollbar);
        track.DecreaseRepeatButton.Opacity.Should().Be(0);
        track.IncreaseRepeatButton.Opacity.Should().Be(0);
        var thumb = (Thumb)track.Thumb;
        thumb.ApplyTemplate();
        var surface = (Border)thumb.Template.FindName("thumbSurface", thumb);
        surface.CornerRadius.Should().Be(new CornerRadius(4));
        surface.Margin.Should().Be(new Thickness(3));
        surface.Background.Should().BeSameAs(scrollbar.FindResource("RepositoryScrollBarThumb"));
    }

    [WpfTheory]
    [InlineData("Dark", "\U0001F441", false, false)]
    [InlineData("Dark", "\U0001F441", true, false)]
    [InlineData("Dark", "\U0001F441", true, true)]
    [InlineData("Light", "\U0001F441", false, false)]
    [InlineData("Light", "\U0001F441", true, false)]
    [InlineData("Light", "\U0001F441", true, true)]
    [InlineData("Dark", "\u2605", false, false)]
    [InlineData("Dark", "\u2605", true, false)]
    [InlineData("Dark", "\u2605", true, true)]
    [InlineData("Light", "\u2605", false, false)]
    [InlineData("Light", "\u2605", true, false)]
    [InlineData("Light", "\u2605", true, true)]
    [InlineData("Dark", "services", false, false)]
    [InlineData("Dark", "services", true, false)]
    [InlineData("Dark", "services", true, true)]
    [InlineData("Light", "services", false, false)]
    [InlineData("Light", "services", true, false)]
    [InlineData("Light", "services", true, true)]
    public void QuickFilter_ShouldUseThemeColorsAndPreserveBuiltInSymbols(string theme, string filterLabel, bool active, bool inverse)
    {
        var bar = new QuickFilterBar();
        var items = (ItemsControl)bar.FindName("quickFilterItemsControl");
        var presenter = new ContentPresenter
        {
            ContentTemplate = items.ItemTemplate,
            Content = new QuickFilterViewModel(new QuickFilterModel
            {
                Label = filterLabel, IsBuiltIn = filterLabel != "services", IsActive = active, IsInverse = inverse,
            }, A.Fake<IQuickFilterService>()),
        };
        presenter.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri($"/RepoM;component/Themes/WorktreePreview.{theme}.xaml", UriKind.Relative),
        });
        presenter.Measure(new System.Windows.Size(100, 100));
        presenter.Arrange(new Rect(presenter.DesiredSize));
        presenter.UpdateLayout();

        var border = (Border)items.ItemTemplate.FindName("qfBorder", presenter);
        var symbol = (Grid)items.ItemTemplate.FindName("monitoringSymbol", presenter);
        var label = (TextBlock)items.ItemTemplate.FindName("filterLabel", presenter);
        if (filterLabel == "\U0001F441")
        {
            label.Visibility.Should().Be(Visibility.Collapsed);
            symbol.Visibility.Should().Be(Visibility.Visible);
            border.BorderThickness.Should().Be(new Thickness(2));
            border.BorderBrush.Should().BeSameAs(presenter.FindResource("RepositoryMonitoringBorder"));
            var corner = FindChild<System.Windows.Shapes.Path>(symbol);
            corner.Fill.Should().BeSameAs(presenter.FindResource("RepositoryMonitoringActive"));
            corner.TranslatePoint(new Point(corner.ActualWidth, 0), border)
                .Should().Be(new Point(border.ActualWidth - 2, 2));
            border.ToolTip.Should().Be("Active");
        }
        else
        {
            label.Visibility.Should().Be(Visibility.Visible);
            symbol.Visibility.Should().Be(Visibility.Collapsed);
            border.BorderThickness.Should().Be(new Thickness(1));
            if (active)
            {
                border.BorderBrush.Should().BeSameAs(presenter.FindResource(
                    inverse ? "QuickFilterInverseBorder" : "QuickFilterSelectedBorder"));
            }
            else
            {
                ((SolidColorBrush)border.BorderBrush).Color.A.Should().Be(0);
            }

            var foreground = filterLabel == "\u2605" ? "PreviewFavorite"
                : !active ? "PreviewMutedForeground"
                : inverse ? "QuickFilterInverseForeground" : "PreviewForeground";
            label.Foreground.Should().BeSameAs(presenter.FindResource(foreground));
        }

        var expectedBackground = active
            ? inverse ? "QuickFilterInverseBackground" : "QuickFilterSelectedBackground"
            : "PreviewTagBackground";
        border.Background.Should().BeSameAs(presenter.FindResource(expectedBackground));
    }

    [WpfTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void FocusingRowButton_ShouldMoveSelectionFromPreviousRepository(bool initiallyExpanded)
    {
        _ = Application.Current ?? new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var monitoring = new RepositoryMonitoringStateService();
        using var first = new RepositoryViewModel(CompactRepositoryInfo(0, 0, 0),
            A.Fake<IFavoriteService>(), monitoring, monitoring);
        using var second = new RepositoryViewModel(CompactRepositoryInfo(0, 0, 0),
            A.Fake<IFavoriteService>(), monitoring, monitoring) { IsExpanded = initiallyExpanded };
        var firstComponent = new RepositoryComponent();
        var secondComponent = new RepositoryComponent();
        var firstRow = new ListBoxItem { DataContext = first, Content = firstComponent };
        var secondRow = new ListBoxItem { DataContext = second, Content = secondComponent };
        var list = new ListBox { SelectionMode = SelectionMode.Single };
        foreach (var resource in new[] { "Themes/WorktreePreview.Dark.xaml", "Themes/RepositoryStyles.xaml", "i18n/en-us.xaml" })
        {
            list.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/RepoM;component/{resource}", UriKind.Relative),
            });
        }

        list.ItemContainerStyle = (Style)list.FindResource("RepositoryRow");
        list.Items.Add(firstRow);
        list.Items.Add(secondRow);
        var window = new Window { Content = list, Width = 400, Height = 400 };
        Appearance.SetEnableAnimations(window, false);
        try
        {
            window.Show();
            window.UpdateLayout();
            list.SelectedItem = firstRow;
            firstRow.IsSelected.Should().BeTrue();

            var arrow = (Button)secondComponent.FindName("DisplayModeButton");
            arrow.Focus().Should().BeTrue();
            arrow.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            second.IsExpanded.Should().Be(!initiallyExpanded);
            firstRow.IsKeyboardFocusWithin.Should().BeFalse();
            firstRow.IsSelected.Should().BeFalse();
            secondRow.IsKeyboardFocusWithin.Should().BeTrue();
            list.SelectedItem.Should().BeSameAs(secondRow);
            list.SelectedItems.Count.Should().Be(1);

            var favorite = (ToggleButton)firstComponent.FindName("FavoriteButton");
            favorite.Focus().Should().BeTrue();
            secondRow.IsSelected.Should().BeFalse();
            secondRow.IsKeyboardFocusWithin.Should().BeFalse();
            list.SelectedItem.Should().BeSameAs(firstRow);
        }
        finally
        {
            window.Close();
        }
    }

    [WpfTheory]
    [InlineData("Dark")]
    [InlineData("Light")]
    public void MonitoringBorder_ShouldFollowStateWithoutReplacingSelectionBackground(string theme)
    {
        var monitoring = new RepositoryMonitoringStateService();
        using var repository = new RepositoryViewModel(CompactRepositoryInfo(0, 0, 0),
            A.Fake<IFavoriteService>(), monitoring, monitoring);
        var component = new RepositoryComponent();
        var row = new ListBoxItem { DataContext = repository, Content = component };
        foreach (var resource in new[] { $"Themes/WorktreePreview.{theme}.xaml", "Themes/RepositoryStyles.xaml" })
        {
            row.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/RepoM;component/{resource}", UriKind.Relative),
            });
        }
        row.Style = (Style)row.FindResource("RepositoryRow");
        row.ApplyTemplate();
        row.Measure(new System.Windows.Size(320, double.PositiveInfinity));
        row.Arrange(new Rect(0, 0, 320, row.DesiredSize.Height));
        row.UpdateLayout();
        var card = (Border)row.Template.FindName("card", row);
        var cornerButton = (ToggleButton)component.FindName("MonitoringButton");
        cornerButton.TranslatePoint(new Point(cornerButton.ActualWidth, 0), card)
            .Should().Be(new Point(card.ActualWidth, 0));
        var corner = (System.Windows.Shapes.Path)cornerButton.Template.FindName("corner", cornerButton);
        corner.Data.FillContains(new Point(13.9, 0.1)).Should().BeFalse("the inner corner must remain rounded");
        corner.Data.FillContains(new Point(11, 4)).Should().BeTrue();
        corner.TranslatePoint(new Point(corner.ActualWidth, 0), card)
            .Should().Be(new Point(card.ActualWidth - card.BorderThickness.Right, card.BorderThickness.Top));
        corner.ActualWidth.Should().Be(14);
        corner.ActualHeight.Should().Be(14);
        ((SolidColorBrush)corner.Fill).Color.A.Should().Be(0);
        corner.IsHitTestVisible.Should().BeTrue();
        ((SolidColorBrush)card.BorderBrush).Color.A.Should().Be(0);
        repository.ToggleMonitoring();
        card.BorderBrush.Should().BeSameAs(row.FindResource("RepositoryMonitoringBorder"));
        corner.Fill.Should().BeSameAs(cornerButton.Foreground);
        ((SolidColorBrush)corner.Fill).Color.Should().Be(((SolidColorBrush)card.BorderBrush).Color);
        card.BorderThickness.Should().Be(new Thickness(2));

        row.IsSelected = true;
        card.Background.Should().BeSameAs(row.FindResource("PreviewSelectedSurface"));
        repository.ToggleMonitoring();
        ((SolidColorBrush)card.BorderBrush).Color.A.Should().Be(0);
        ((SolidColorBrush)corner.Fill).Color.A.Should().Be(0);
        card.Background.Should().BeSameAs(row.FindResource("PreviewSelectedSurface"));
    }

    [WpfTheory]
    [InlineData("Dark")]
    [InlineData("Light")]
    public void CompactRow_ShouldKeepStatusVisibleAtNarrowWidthsAndRefreshInPlace(string theme)
    {
        var monitoring = new RepositoryMonitoringStateService();
        using var repository = new RepositoryViewModel(
            CompactRepositoryInfo(2, 3, 1), A.Fake<IFavoriteService>(),
            monitoring, monitoring)
        {
            IsExpanded = false,
        };
        var component = new RepositoryComponent { DataContext = repository };
        foreach (var resource in new[] { $"Themes/WorktreePreview.{theme}.xaml", "Themes/RepositoryStyles.xaml", "i18n/en-us.xaml" })
        {
            component.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri($"/RepoM;component/{resource}", UriKind.Relative),
            });
        }

        void Layout()
        {
            component.Measure(new System.Windows.Size(320, double.PositiveInfinity));
            component.Arrange(new Rect(0, 0, 320, component.DesiredSize.Height));
            component.UpdateLayout();
        }

        Layout();
        var branch = (Run)component.FindName("CompactBranch");
        var heading = (TextBlock)component.FindName("CompactHeading");
        var ahead = (TextBlock)component.FindName("CompactAhead");
        var behind = (TextBlock)component.FindName("CompactBehind");
        var dirty = (TextBlock)component.FindName("CompactDirty");
        var indicators = (StackPanel)component.FindName("CompactStatus");
        var favorite = (ToggleButton)component.FindName("FavoriteButton");
        var radar = (ToggleButton)component.FindName("MonitoringButton");
        radar.Visibility.Should().Be(Visibility.Visible);
        radar.IsChecked.Should().BeFalse();
        radar.TranslatePoint(new Point(radar.ActualWidth, 0), component).X.Should()
            .BeApproximately(component.ActualWidth, 0.1);
        radar.TranslatePoint(new Point(), component).Y.Should().BeApproximately(0, 0.1);
        var offColor = ((SolidColorBrush)radar.Foreground).Color;
        radar.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        Layout();
        radar.IsChecked.Should().BeTrue();
        ((SolidColorBrush)radar.Foreground).Color.Should().NotBe(offColor);
        branch.Text.Should().Be("   feature/a-very-long-branch-name-that-must-be-truncated");
        heading.ActualWidth.Should().BeGreaterThan(0);
        var name = (Run)heading.Inlines.FirstInline;
        name.NextInline.Should().BeSameAs(branch);
        name.FontSize.Should().Be(17);
        name.FontWeight.Should().Be(FontWeights.SemiBold);
        branch.FontSize.Should().Be(heading.FontSize);
        branch.FontWeight.Should().Be(heading.FontWeight);
        branch.Foreground.Should().Be(heading.Foreground);
        heading.TranslatePoint(new Point(heading.ActualWidth, 0), component).X
            .Should().BeLessThanOrEqualTo(indicators.TranslatePoint(new Point(), component).X);
        ahead.Text.Should().Be("\u21912");
        behind.Text.Should().Be("\u21933");
        dirty.Visibility.Should().Be(Visibility.Visible);
        ((SolidColorBrush)dirty.Foreground).Color.Should().NotBe(((SolidColorBrush)ahead.Foreground).Color);
        indicators.ActualWidth.Should().BeApproximately(indicators.DesiredSize.Width, 0.1);
        var rightEdge = indicators.TranslatePoint(new Point(indicators.ActualWidth, 0), component).X;
        rightEdge.Should().BeLessThanOrEqualTo(favorite.TranslatePoint(new Point(), component).X);
        ((StackPanel)component.FindName("EntryDetails")).Visibility.Should().Be(Visibility.Collapsed);
        double compactHeight = component.ActualHeight;

        repository.Update(CompactRepositoryInfo(0, 0, 0));
        Layout();
        ahead.Visibility.Should().Be(Visibility.Collapsed);
        behind.Visibility.Should().Be(Visibility.Collapsed);
        dirty.Visibility.Should().Be(Visibility.Collapsed);
        ((TextBlock)component.FindName("CompactUnknown")).Visibility.Should().Be(Visibility.Collapsed);
        ((TextBlock)component.FindName("CompactNoUpstream")).Visibility.Should().Be(Visibility.Collapsed);

        RepositoryInfo unknown = CompactRepositoryInfo(0, 0, 0);
        unknown.LocalModified = null;
        repository.Update(unknown);
        Layout();
        ((TextBlock)component.FindName("CompactUnknown")).Visibility.Should().Be(Visibility.Visible);
        RepositoryInfo noUpstream = CompactRepositoryInfo(0, 0, 0);
        noUpstream.CurrentBranchHasUpstream = false;
        repository.Update(noUpstream);
        Layout();
        ((TextBlock)component.FindName("CompactNoUpstream")).Visibility.Should().Be(Visibility.Visible);

        repository.IsExpanded = true;
        repository.Update(CompactRepositoryInfo(2, 3, 1));
        Layout();
        radar.Visibility.Should().Be(Visibility.Visible);
        radar.IsChecked.Should().BeTrue();
        radar.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        repository.EnableMonitoring();
        Layout();
        radar.IsChecked.Should().BeFalse("automatic menu preparation must respect manual opt-out");
        ((SolidColorBrush)radar.Foreground).Color.Should().Be(offColor);
        indicators.Visibility.Should().Be(Visibility.Visible);
        ahead.Visibility.Should().Be(Visibility.Visible);
        behind.Visibility.Should().Be(Visibility.Visible);
        dirty.Visibility.Should().Be(Visibility.Visible);
        indicators.ActualWidth.Should().BeApproximately(indicators.DesiredSize.Width, 0.1);
        indicators.TranslatePoint(new Point(indicators.ActualWidth, 0), component).X
            .Should().BeLessThanOrEqualTo(favorite.TranslatePoint(new Point(), component).X);
        heading.Visibility.Should().Be(Visibility.Collapsed);
        ((StackPanel)component.FindName("EntryDetails")).Visibility.Should().Be(Visibility.Visible);
        component.ActualHeight.Should().BeGreaterThan(compactHeight);

        repository.Update(CompactRepositoryInfo(0, 0, 0));
        Layout();
        ahead.Visibility.Should().Be(Visibility.Collapsed);
        behind.Visibility.Should().Be(Visibility.Collapsed);
        dirty.Visibility.Should().Be(Visibility.Collapsed);
    }

    private static RepositoryInfo CompactRepositoryInfo(int ahead, int behind, int modified) => new()
    {
        Name = "A very long repository name that must be truncated",
        Path = @"C:\Repos\Real", SafePath = "C:/Repos/Real",
        CurrentBranch = "feature/a-very-long-branch-name-that-must-be-truncated",
        CurrentBranchHasUpstream = true, AheadBy = ahead, BehindBy = behind,
        LocalAdded = 0, LocalStaged = 0, LocalRemoved = 0,
        LocalModified = modified, LocalMissing = 0, LocalUntracked = 0,
    };

    private static MemoryStream CreatePng(int width, int height, byte[] pixels)
    {
        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        var stream = new MemoryStream();
        encoder.Save(stream);
        stream.Position = 0;
        return stream;
    }

    [WpfFact]
    public async Task MainWindow_ShouldRenderRealRepositoryCardsAndPersistHamburgerAppearanceChoices()
    {
        // WPF permits only one Application per test process; shutting it down also disables later resource loads.
        _ = Application.Current ?? new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var files = new MockFileSystem();
        var paths = A.Fake<IAppDataPathProvider>();
        A.CallTo(() => paths.AppDataPath).Returns(@"C:\TestSettings");
        var settings = new FileAppSettingsService(paths, files, NullLogger.Instance)
        {
            Theme = AppTheme.Light,
            EnableAnimations = false,
        };
        using var store = new RepositoryStore();
        var favorites = A.Fake<IFavoriteService>();
        var monitoring = A.Fake<IRepositoryMonitoringService>();
        var quickFilters = A.Fake<IQuickFilterService>();
        A.CallTo(() => quickFilters.FindByQuery(A<IQuery>._)).Returns(null);
        var menuFactory = A.Fake<IUserMenuActionMenuFactory>();
        var window = new MainWindow(RepositoryListViewModelTests.CreateMonitorService(), store, favorites, monitoring,
            A.Fake<IRepositoryMonitoringEvents>(), A.Fake<IRepositoryIgnoreStore>(), settings,
            A.Fake<ITranslationService>(), A.Fake<IAppDataPathProvider>(), new MockFileSystem(),
            new ActionExecutor(new SimpleInjector.Container(), NullLogger.Instance), CreateOrdering(), A.Fake<IThreadDispatcher>(),
            CreateFiltering(), quickFilters, [A.Fake<INamedQueryParser>()], A.Fake<IModuleManager>(),
            NullLogger.Instance, menuFactory);
        window.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("/RepoM;component/i18n/en-us.xaml", UriKind.Relative),
        });

        try
        {
            window.SetReady();
            window.Show();
            store.AddOrUpdate(new RepositoryInfo
            {
                Name = "Real repository",
                Path = @"C:\Repos\Real",
                SafePath = "C:/Repos/Real",
                CurrentBranch = "main",
                Branches = ["main"],
                Tags = ["work"],
                WasFound = true,
            });
            var repositories = (RepositoryList)window.FindName("repositoryList");
            var list = (ListBox)repositories.FindName("lstRepositories");
            for (int attempt = 0; list.Items.Count == 0 && attempt < 100; attempt++)
            {
                await Task.Delay(50);
                await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            }

            list.Items.Count.Should().Be(1);
            var repository = (RepositoryViewModel)list.Items[0];
            var row = (ListBoxItem)list.ItemContainerGenerator.ContainerFromIndex(0);
            var component = FindChild<RepositoryComponent>(row);
            // Consume any prefetch caused by the real mouse position when the window appeared.
            list.SelectedItem = repository;
            await ((RepositoryListViewModel)repositories.DataContext).CreateContextMenuEntriesAsync(CancellationToken.None);
            int menuCallsBeforeHover = Fake.GetCalls(menuFactory).Count(call => call.Method.Name == nameof(IUserMenuActionMenuFactory.CreateMenuAsync));
            ((Border)component.Content).RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
            await Task.Delay(400);
            Fake.GetCalls(menuFactory).Count(call => call.Method.Name == nameof(IUserMenuActionMenuFactory.CreateMenuAsync))
                .Should().BeGreaterThan(menuCallsBeforeHover);
            var title = (TextBlock)component.FindName("EntryName");
            title.Text.Should().Be("Real repository");
            var card = (Border)row.Template.FindName("card", row);
            ((SolidColorBrush)card.Background).Color.R.Should().BeGreaterThan(200);
            list.SelectedItem = repository;

            var button = (Button)component.FindName("DisplayModeButton");
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            window.UpdateLayout();
            repository.IsExpanded.Should().BeFalse();
            component.HasAnimatedProperties.Should().BeFalse();
            ((StackPanel)component.FindName("EntryDetails")).Visibility.Should().Be(Visibility.Collapsed);
            double titleLeft = ((TextBlock)component.FindName("CompactHeading")).TranslatePoint(new Point(), row).X;
            double minimalHeight = component.ActualHeight;

            var animations = (MenuItem)window.FindName("AnimationsMenuItem");
            animations.SetCurrentValue(MenuItem.IsCheckedProperty, true);
            Appearance.GetEnableAnimations(component).Should().BeTrue();
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            window.UpdateLayout();
            component.HasAnimatedProperties.Should().BeTrue();
            title.TranslatePoint(new Point(), row).X.Should().Be(titleLeft);
            await Task.Delay(300);
            window.UpdateLayout();
            component.ActualHeight.Should().BeGreaterThan(minimalHeight);
            component.HasAnimatedProperties.Should().BeFalse();
            list.SelectedItem.Should().BeSameAs(repository);

            ((ToggleButton)component.FindName("FavoriteButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            A.CallTo(() => favorites.SetFavorite("C:/Repos/Real", true)).MustHaveHappenedOnceExactly();
            var monitoringButton = (ToggleButton)component.FindName("MonitoringButton");
            Keyboard.Focus(button);
            var previousFocus = Keyboard.FocusedElement;
            Keyboard.Focus(monitoringButton);
            monitoringButton.IsKeyboardFocused.Should().BeFalse();
            Keyboard.FocusedElement.Should().BeSameAs(previousFocus);
            monitoringButton.IsTabStop.Should().BeFalse();
            var monitoringCorner = (System.Windows.Shapes.Path)monitoringButton.Template.FindName("corner", monitoringButton);
            monitoringCorner.Stroke.Should().BeNull("focus must not draw an outline around the triangle");
            var tags = (ItemsControl)component.FindName("Tags");
            var tag = FindChild<Button>(tags.ItemContainerGenerator.ContainerFromIndex(0));
            ((IInvokeProvider)new ButtonAutomationPeer(tag).GetPattern(PatternInterface.Invoke)).Invoke();
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            A.CallTo(() => quickFilters.Add("work", A<IQuery>._)).MustHaveHappenedOnceExactly();

            var hamburger = (Button)window.FindName("MenuButton");
            hamburger.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var menu = (ContextMenu)window.FindName("SettingsMenu");
            menu.IsOpen.Should().BeTrue();
            var appearance = (MenuItem)window.FindName("AppearanceMenuItem");
            appearance.IsSubmenuOpen = true;
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            var dark = (MenuItem)window.FindName("ThemeDarkMenuItem");
            dark.SetCurrentValue(MenuItem.IsCheckedProperty, true);
            menu.IsOpen.Should().BeFalse();
            ((SolidColorBrush)window.Background).Color.R.Should().BeLessThan(100);
            ((MenuItem)window.FindName("ThemeLightMenuItem")).IsChecked.Should().BeFalse();
            dark.SetCurrentValue(MenuItem.IsCheckedProperty, false);
            dark.IsChecked.Should().BeTrue();
            var system = (MenuItem)window.FindName("ThemeSystemMenuItem");
            system.SetCurrentValue(MenuItem.IsCheckedProperty, true);
            system.IsChecked.Should().BeTrue();
            dark.IsChecked.Should().BeFalse();
            animations.SetCurrentValue(MenuItem.IsCheckedProperty, false);
            Appearance.GetEnableAnimations(component).Should().BeFalse();

            var reloaded = new FileAppSettingsService(paths, files, NullLogger.Instance);
            reloaded.Theme.Should().Be(AppTheme.System);
            reloaded.EnableAnimations.Should().BeFalse();
            list.SelectedItem.Should().BeSameAs(repository);
        }
        finally
        {
            window.Shutdown();
        }
    }

    private static T FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
            {
                return match;
            }

            if (TryFindChild<T>(child) is T descendant)
            {
                return descendant;
            }
        }

        throw new InvalidOperationException($"No {typeof(T).Name} found.");
    }

    private static T? TryFindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
            {
                return match;
            }

            if (TryFindChild<T>(child) is T descendant)
            {
                return descendant;
            }
        }

        return null;
    }

    private static IRepositoryComparerManager CreateOrdering()
    {
        var manager = A.Fake<IRepositoryComparerManager>();
        A.CallTo(() => manager.SortObservable).Returns(Observable.Return(
            Comparer<RepositoryViewModel>.Create((left, right) => StringComparer.Ordinal.Compare(left.Name, right.Name))));
        return manager;
    }

    private static IRepositoryFilteringManager CreateFiltering()
    {
        var manager = A.Fake<IRepositoryFilteringManager>();
        A.CallTo(() => manager.CreateFilterObservable(A<IObservable<string>>._))
            .Returns(Observable.Return<Func<RepositoryViewModel, bool>>(_ => true));
        return manager;
    }
}
