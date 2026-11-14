namespace RepoM.App.Tests;

using System;
using System.Collections.Generic;
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
using System.Windows.Media;
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
using Xunit;

[DisableParallelization]
public class RepositoryFrontendTests
{
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
            await Task.Delay(500);
            await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);

            var repositories = (RepositoryList)window.FindName("repositoryList");
            var list = (ListBox)repositories.FindName("lstRepositories");
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
            Point titlePosition = title.TranslatePoint(new Point(), row);
            double minimalHeight = component.ActualHeight;

            var animations = (MenuItem)window.FindName("AnimationsMenuItem");
            animations.SetCurrentValue(MenuItem.IsCheckedProperty, true);
            Appearance.GetEnableAnimations(component).Should().BeTrue();
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            window.UpdateLayout();
            component.HasAnimatedProperties.Should().BeTrue();
            title.TranslatePoint(new Point(), row).Should().Be(titlePosition);
            await Task.Delay(300);
            window.UpdateLayout();
            component.ActualHeight.Should().BeGreaterThan(minimalHeight);
            component.HasAnimatedProperties.Should().BeFalse();
            list.SelectedItem.Should().BeSameAs(repository);

            ((ToggleButton)component.FindName("FavoriteButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            A.CallTo(() => favorites.SetFavorite("C:/Repos/Real", true)).MustHaveHappenedOnceExactly();
            ((ToggleButton)component.FindName("MonitoringButton")).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            A.CallTo(() => monitoring.SetMonitored("C:/Repos/Real", true)).MustHaveHappenedOnceExactly();
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
