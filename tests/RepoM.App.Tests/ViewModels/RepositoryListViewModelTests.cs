namespace RepoM.App.Tests.ViewModels;

using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using DynamicData.Kernel;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using RepoM.ActionMenu.Interface.ActionMenuFactory;
using RepoM.ActionMenu.Interface.UserInterface;
using RepoM.Api.Git;
using RepoM.App.RepositoryActions;
using RepoM.App.Services;
using RepoM.App.ViewModels;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryActions;
using RepoM.Core.Repositories;
using RepoM.Core.Repositories.Favorite;
using RepoM.Core.Repositories.Model;
using RepoM.Core.Repositories.Monitoring;
using RepoM.Core.Repositories.Reading;
using RepoM.Core.Repositories.Scanning;
using RepoM.Core.Repositories.Store;
using RepoM.Core.Repositories.Watching;
using SimpleInjector;
using Xunit;

public class RepositoryListViewModelTests
{
    [Fact]
    public void Ctor_ShouldThrow_WhenArgumentNull()
    {
        // arrange
        var monitorService = CreateMonitorService();
        var executor = CreateActionExecutor(_ => { });
        var menuFactory = A.Fake<IUserMenuActionMenuFactory>();
        var command = A.Fake<System.Windows.Input.ICommand>();

        // act
        Action act1 = () => _ = new RepositoryListViewModel(null!, executor, menuFactory, NullLogger.Instance, command);
        Action act2 = () => _ = new RepositoryListViewModel(monitorService, null!, menuFactory, NullLogger.Instance, command);
        Action act3 = () => _ = new RepositoryListViewModel(monitorService, executor, null!, NullLogger.Instance, command);
        Action act4 = () => _ = new RepositoryListViewModel(monitorService, executor, menuFactory, null!, command);
        Action act5 = () => _ = new RepositoryListViewModel(monitorService, executor, menuFactory, NullLogger.Instance, null!);

        // assert
        act1.Should().ThrowExactly<ArgumentNullException>();
        act2.Should().ThrowExactly<ArgumentNullException>();
        act3.Should().ThrowExactly<ArgumentNullException>();
        act4.Should().ThrowExactly<ArgumentNullException>();
        act5.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void SettingProperties_ShouldRaisePropertyChanged()
    {
        // arrange
        var sut = CreateSut();
        var propertyNames = new List<string?>();
        sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName);
        var items = new[] { "a", "b" };
        var repository = CreateRepositoryViewModel();

        // act
        sut.ItemsSource = items;
        sut.SelectedRepository = repository;

        // assert
        propertyNames.Should().Contain(nameof(RepositoryListViewModel.ItemsSource));
        propertyNames.Should().Contain(nameof(RepositoryListViewModel.SelectedRepository));
        sut.ItemsSource.Should().BeSameAs(items);
        sut.SelectedRepository.Should().BeSameAs(repository);
    }

    [Fact]
    public async Task CreateContextMenuEntriesAsync_ShouldReturnEmpty_WhenNothingSelected()
    {
        // arrange
        var sut = CreateSut();

        // act
        var result = await sut.CreateContextMenuEntriesAsync(default);

        // assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateContextMenuEntriesAsync_ShouldCreateEntries_ForRegularAndDeferredActions()
    {
        // arrange
        var repository = CreateRepositoryViewModel();
        var updatedInfo = CreateRepositoryInfo(currentBranch: "develop");
        var monitorService = CreateMonitorService(updatedInfo);
        var menuFactory = A.Fake<IUserMenuActionMenuFactory>();
        var context = A.Fake<IActionMenuGenerationContext>();
        A.CallTo(() => context.Clone()).Returns(context);

        var regularAction = new UserInterfaceRepositoryAction("Open", repository.Repository)
        {
            RepositoryCommand = new TestRepositoryCommand(),
        };

        var deferredAction = new DeferredSubActionsUserInterfaceRepositoryAction(
            "More",
            repository.Repository,
            context,
            _ => Task.FromResult<UserInterfaceRepositoryActionBase[]>(
            [
                new UserInterfaceRepositoryAction("SubAction", repository.Repository)
                {
                    RepositoryCommand = new TestRepositoryCommand(),
                },
            ]));

        A.CallTo(() => menuFactory.CreateMenuAsync(A<IRepository>._)).Returns(GetMenuActions(repository.Repository,
        [
            new UserInterfaceSeparatorRepositoryAction(repository.Repository),
            regularAction,
            deferredAction,
        ]));

        var sut = CreateSut(monitorService: monitorService, menuFactory: menuFactory);
        sut.SelectedRepository = repository;

        // act
        var result = await sut.CreateContextMenuEntriesAsync(default);

        // assert
        result.Should().HaveCount(3);
        result[0].Should().BeSameAs(RepositoryMenuSeparatorViewModel.Instance);

        result[1].Should().BeOfType<RepositoryMenuItemViewModel>();
        var regularEntry = (RepositoryMenuItemViewModel)result[1];
        regularEntry.Header.Should().Be("Open");
        regularEntry.IsEnabled.Should().BeTrue();
        regularEntry.HasSubItems.Should().BeFalse();

        result[2].Should().BeOfType<RepositoryMenuItemViewModel>();
        var deferredEntry = (RepositoryMenuItemViewModel)result[2];
        deferredEntry.HasSubItems.Should().BeTrue();
        IReadOnlyList<RepositoryMenuEntryViewModel> childEntries = await deferredEntry.LoadChildrenAsync();
        childEntries.Should().ContainSingle();
        childEntries[0].Should().BeOfType<RepositoryMenuItemViewModel>();
        ((RepositoryMenuItemViewModel)childEntries[0]).Header.Should().Be("SubAction");

        A.CallTo(() => A.Fake<IRepositoryMonitoringService>().EnableMonitoring(A<string>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task CreateContextMenuEntriesAsync_ShouldEnableMonitoring_ForSelectedRepository()
    {
        // arrange
        var monitoringService = A.Fake<IRepositoryMonitoringService>();
        var repository = CreateRepositoryViewModel(monitoringService: monitoringService);
        var menuFactory = A.Fake<IUserMenuActionMenuFactory>();
        A.CallTo(() => menuFactory.CreateMenuAsync(A<IRepository>._))
            .Returns(GetMenuActions(repository.Repository, [new UserInterfaceRepositoryAction("Open", repository.Repository)]));
        var sut = CreateSut(menuFactory: menuFactory);
        sut.SelectedRepository = repository;

        // act
        _ = await sut.CreateContextMenuEntriesAsync(default);

        // assert
        A.CallTo(() => monitoringService.EnableMonitoring(repository.Repository.SafePath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InvokeDefaultActionOnSelectionAsync_ShouldNotCreateMenu_WhenRepositoryWasNotFound()
    {
        // arrange
        var repository = CreateRepositoryViewModel(info: CreateRepositoryInfo(wasFound: false));
        var menuFactory = A.Fake<IUserMenuActionMenuFactory>();
        var sut = CreateSut(menuFactory: menuFactory);
        sut.SelectedRepository = repository;

        // act
        await sut.InvokeDefaultActionOnSelectionAsync();

        // assert
        A.CallTo(() => menuFactory.CreateMenuAsync(A<IRepository>._)).MustNotHaveHappened();
    }

    private static RepositoryListViewModel CreateSut(
        RepositoryMonitorService? monitorService = null,
        IUserMenuActionMenuFactory? menuFactory = null,
        ActionExecutor? executor = null)
    {
        return new RepositoryListViewModel(
            monitorService ?? CreateMonitorService(),
            executor ?? CreateActionExecutor(_ => { }),
            menuFactory ?? A.Fake<IUserMenuActionMenuFactory>(),
            NullLogger.Instance,
            A.Dummy<System.Windows.Input.ICommand>());
    }

    private static RepositoryMonitorService CreateMonitorService(RepositoryInfo? updatedInfo = null)
    {
        var scanner = A.Fake<IRepositoryScanner>();
        var watcher = A.Fake<IRepositoryWatcher>();
        var reader = A.Fake<IRepositoryInfoReader>();
        var store = new RepositoryStore();
        var fileSystem = new MockFileSystem();
        var monitoringState = A.Fake<IRepositoryMonitoringService>();
        var monitoringEvents = A.Fake<IRepositoryMonitoringEvents>();

        if (updatedInfo != null)
        {
            A.CallTo(() => reader.ReadAsync(updatedInfo.Path, default)).Returns(updatedInfo);
        }

        return new RepositoryMonitorService(
            scanner,
            watcher,
            reader,
            store,
            fileSystem,
            () => [],
            monitoringState,
            monitoringEvents,
            NullLogger.Instance);
    }

    private static ActionExecutor CreateActionExecutor(Action<TestRepositoryCommand> onExecute)
    {
        var container = new Container();
        container.RegisterInstance<ICommandExecutor<TestRepositoryCommand>>(new TestRepositoryCommandExecutor(onExecute));
        return new ActionExecutor(container, NullLogger.Instance);
    }

    private static RepositoryViewModel CreateRepositoryViewModel(
        RepositoryInfo? info = null,
        IFavoriteService? favoriteService = null,
        IRepositoryMonitoringService? monitoringService = null,
        IRepositoryMonitoringEvents? monitoringEvents = null)
    {
        return new RepositoryViewModel(
            info ?? CreateRepositoryInfo(),
            favoriteService ?? A.Fake<IFavoriteService>(),
            monitoringService ?? A.Fake<IRepositoryMonitoringService>(),
            monitoringEvents ?? A.Fake<IRepositoryMonitoringEvents>());
    }

    private static RepositoryInfo CreateRepositoryInfo(string currentBranch = "main", bool wasFound = true)
    {
        return new RepositoryInfo
        {
            Path = @"C:\Repos\RepoM",
            SafePath = "C:/Repos/RepoM",
            Name = "RepoM",
            CurrentBranch = currentBranch,
            Branches = [currentBranch],
            LocalBranches = [currentBranch],
            Tags = ["work"],
            LastUpdated = DateTimeOffset.UtcNow.AddMinutes(-10),
            LastSeen = DateTimeOffset.UtcNow.AddMinutes(-10),
            WasFound = wasFound,
        };
    }

    private static async IAsyncEnumerable<UserInterfaceRepositoryActionBase> GetMenuActions(IRepository repository, IEnumerable<UserInterfaceRepositoryActionBase> actions)
    {
        _ = repository;
        foreach (UserInterfaceRepositoryActionBase action in actions)
        {
            yield return action;
            await Task.Yield();
        }
    }

    private sealed class TestRepositoryCommand : IRepositoryCommand
    {
    }

    private sealed class TestRepositoryCommandExecutor : ICommandExecutor<TestRepositoryCommand>
    {
        private readonly Action<TestRepositoryCommand> _onExecute;

        public TestRepositoryCommandExecutor(Action<TestRepositoryCommand> onExecute)
        {
            _onExecute = onExecute;
        }

        public void Execute(IRepository repository, TestRepositoryCommand action)
        {
            _ = repository;
            _onExecute(action);
        }
    }
}